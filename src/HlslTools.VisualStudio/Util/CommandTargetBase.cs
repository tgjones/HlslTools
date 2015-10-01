using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace HlslTools.VisualStudio.Util
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

        protected abstract bool IsEnabled(TCommandEnum commandId);
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

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup != CommandGroup)
                return NextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);

            for (int i = 0; i < cCmds; i++)
            {
                if (CommandIds.Contains(prgCmds[i].cmdID))
                {
                    if (IsEnabled((TCommandEnum)(object)(int)prgCmds[i].cmdID))
                    {
                        prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                        return VSConstants.S_OK;
                    }

                    prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
                }
            }

            return NextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }
    }
}