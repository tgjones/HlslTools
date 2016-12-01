using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using ShaderTools.VisualStudio.Core.Util;

namespace ShaderTools.VisualStudio.Hlsl.Editing.Commenting
{
    internal sealed class CommentCommandTarget : CommandTargetBase<VSConstants.VSStd2KCmdID>
    {
        public CommentCommandTarget(IVsTextView adapter, IWpfTextView textView)
            : base(adapter, textView, VSConstants.VSStd2KCmdID.COMMENTBLOCK, VSConstants.VSStd2KCmdID.COMMENT_BLOCK)
        {
        }

        protected override bool Execute(VSConstants.VSStd2KCmdID commandId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            return TextView.CommentOrUncommentBlock(comment: true);
        }

        protected override bool IsEnabled(VSConstants.VSStd2KCmdID commandId, ref string commandText)
        {
            return true;
        }
    }
}