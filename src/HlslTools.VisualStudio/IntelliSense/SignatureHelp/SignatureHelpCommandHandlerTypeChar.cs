using System;
using System.Runtime.InteropServices;
using HlslTools.VisualStudio.Util;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace HlslTools.VisualStudio.IntelliSense.SignatureHelp
{
    internal sealed class SignatureHelpCommandHandlerTypeChar : CommandTargetBase<VSConstants.VSStd2KCmdID>
    {
        private readonly SignatureHelpManager _signatureHelpManager;

        public SignatureHelpCommandHandlerTypeChar(IVsTextView adapter, IWpfTextView textView, SignatureHelpManager signatureHelpManager)
            : base(adapter, textView, VSConstants.VSStd2KCmdID.TYPECHAR)
        {
            _signatureHelpManager = signatureHelpManager;
        }

        public override int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return NextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public override int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            var result = NextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

            if (result == VSConstants.S_OK && pguidCmdGroup == CommandGroup && CommandIds.Contains(nCmdID))
            {
                var typedChar = (char) (ushort) Marshal.GetObjectForNativeVariant(pvaIn);
                _signatureHelpManager.HandleTextInput(typedChar.ToString());
            }

            return result;
        }

        protected override bool IsEnabled(VSConstants.VSStd2KCmdID commandId, ref string commandText)
        {
            throw new NotSupportedException();
        }

        protected override bool Execute(VSConstants.VSStd2KCmdID commandId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            throw new NotSupportedException();
        }
    }
}