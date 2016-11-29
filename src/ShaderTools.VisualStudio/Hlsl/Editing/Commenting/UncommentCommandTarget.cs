using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using ShaderTools.VisualStudio.Hlsl.Util;

namespace ShaderTools.VisualStudio.Hlsl.Editing.Commenting
{
    internal sealed class UncommentCommandTarget : CommandTargetBase<VSConstants.VSStd2KCmdID>
    {
        public UncommentCommandTarget(IVsTextView adapter, IWpfTextView textView)
            : base(adapter, textView, VSConstants.VSStd2KCmdID.UNCOMMENTBLOCK, VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK)
        {
        }

        protected override bool Execute(VSConstants.VSStd2KCmdID commandId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            return TextView.CommentOrUncommentBlock(comment: false);
        }

        protected override bool IsEnabled(VSConstants.VSStd2KCmdID commandId, ref string commandText)
        {
            return true;
        }
    }
}