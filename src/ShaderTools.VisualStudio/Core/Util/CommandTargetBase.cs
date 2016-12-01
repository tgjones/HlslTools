using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace ShaderTools.VisualStudio.Core.Util
{
    internal abstract class CommandTargetBase<TCommandEnum> : IOleCommandTarget
        where TCommandEnum : struct, IComparable
    {
        protected IOleCommandTarget NextCommandTarget;
        protected readonly IWpfTextView TextView;

        public Guid CommandGroup { get; }
        public ReadOnlyCollection<uint> CommandIds { get; }

        protected CommandTargetBase(IVsTextView adapter, IWpfTextView textView, params TCommandEnum[] commandIds)
            : this(adapter, textView, typeof(TCommandEnum).GUID, Array.ConvertAll(commandIds, e => Convert.ToUInt32(e, CultureInfo.InvariantCulture)))
        {
            
        }

        protected CommandTargetBase(IVsTextView adapter, IWpfTextView textView, Guid commandGroup, params uint[] commandIds)
        {
            CommandGroup = commandGroup;
            CommandIds = new ReadOnlyCollection<uint>(commandIds);
            TextView = textView;

            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                // Add the target later to make sure it makes it in before other command handlers
                ErrorHandler.ThrowOnFailure(adapter.AddCommandFilter(this, out NextCommandTarget));
            }, DispatcherPriority.ApplicationIdle);
        }

        protected abstract bool IsEnabled(TCommandEnum commandId, ref string commandText);
        protected abstract bool Execute(TCommandEnum commandId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut);

        public virtual int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == CommandGroup && CommandIds.Contains(nCmdID))
            {
                var result = Execute((TCommandEnum)(object)(int)nCmdID, nCmdexecopt, pvaIn, pvaOut);

                if (result)
                    return VSConstants.S_OK;
            }

            return NextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        public virtual int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup != CommandGroup)
                return NextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);

            for (int i = 0; i < cCmds; i++)
            {
                if (CommandIds.Contains(prgCmds[i].cmdID))
                {
                    var commandText = GetCommandText(pCmdText);
                    var originalCommandText = commandText;

                    if (IsEnabled((TCommandEnum)(object)(int)prgCmds[i].cmdID, ref commandText))
                    {
                        prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                        if (pCmdText != IntPtr.Zero && commandText != originalCommandText)
                            SetCommandText(pCmdText, commandText);
                        return VSConstants.S_OK;
                    }

                    prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
                }
            }

            return NextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        // From https://github.com/tunnelvisionlabs/InheritanceMargin/blob/master/Tvl.VisualStudio.InheritanceMargin/CommandTranslation/CommandRouter.cs#L108
        private static string GetCommandText(IntPtr structPtr)
        {
            if (structPtr == IntPtr.Zero)
                return string.Empty;

            OLECMDTEXT olecmdtext = (OLECMDTEXT)Marshal.PtrToStructure(structPtr, typeof(OLECMDTEXT));
            if (olecmdtext.cwActual == 0)
                return string.Empty;

            IntPtr offset = Marshal.OffsetOf(typeof(OLECMDTEXT), "rgwz");
            IntPtr ptr = (IntPtr)((long)structPtr + (long)offset);
            return Marshal.PtrToStringUni(ptr, (int)olecmdtext.cwActual - 1);
        }

        // From https://github.com/tunnelvisionlabs/InheritanceMargin/blob/master/Tvl.VisualStudio.InheritanceMargin/CommandTranslation/CommandRouter.cs#L122
        private static void SetCommandText(IntPtr pCmdTextInt, string text)
        {
            if (text != null)
            {
                OLECMDTEXT olecmdtext = (OLECMDTEXT)Marshal.PtrToStructure(pCmdTextInt, typeof(OLECMDTEXT));
                if ((olecmdtext.cmdtextf & (uint)OLECMDTEXTF.OLECMDTEXTF_NAME) == 0)
                    return;

                var source = text.ToCharArray();
                var bufferOffset = Marshal.OffsetOf(typeof(OLECMDTEXT), "rgwz");
                var lengthOffset = Marshal.OffsetOf(typeof(OLECMDTEXT), "cwActual");
                var length = Math.Min(((int)olecmdtext.cwBuf) - 1, source.Length);

                // copy the new text
                var bufferAddress = (long)pCmdTextInt + (long)bufferOffset;
                Marshal.Copy(source, 0, (IntPtr)bufferAddress, length);
                // null terminator
                Marshal.WriteInt16(pCmdTextInt, (int)bufferOffset + length * 2, 0);
                // length including null terminator
                Marshal.WriteInt32(pCmdTextInt, (int)lengthOffset, length + 1);
            }
        }
    }
}