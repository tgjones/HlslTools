using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using ShaderTools.VisualStudio.Hlsl.Util;

namespace ShaderTools.VisualStudio.Hlsl.IntelliSense.SignatureHelp
{
    internal sealed class SignatureHelpCommandHandlerParamInfo : CommandTargetBase<VSConstants.VSStd2KCmdID>
    {
        private readonly SignatureHelpManager _signatureHelpManager;

        public SignatureHelpCommandHandlerParamInfo(IVsTextView adapter, IWpfTextView textView, SignatureHelpManager signatureHelpManager)
            : base(adapter, textView, VSConstants.VSStd2KCmdID.PARAMINFO)
        {
            _signatureHelpManager = signatureHelpManager;
        }

        protected override bool IsEnabled(VSConstants.VSStd2KCmdID commandId, ref string commandText)
        {
            return true;
        }

        protected override bool Execute(VSConstants.VSStd2KCmdID commandId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            _signatureHelpManager.TriggerSignatureHelp();
            return true;
        }
    }
}