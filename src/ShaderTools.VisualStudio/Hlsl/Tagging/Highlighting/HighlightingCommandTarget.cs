using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using ShaderTools.VisualStudio.Hlsl.Util;

namespace ShaderTools.VisualStudio.Hlsl.Tagging.Highlighting
{
    internal sealed class HighlightingCommandTarget : CommandTargetBase<VSConstants.VSStd2KCmdID>
    {
        private const VSConstants.VSStd2KCmdID CmdidNextHighlightedReference = (VSConstants.VSStd2KCmdID)2400;
        private const VSConstants.VSStd2KCmdID CmdidPreviousHighlightedReference = (VSConstants.VSStd2KCmdID)2401;

        private readonly HighlightingNavigationManager _navigationManager;

        public HighlightingCommandTarget(IVsTextView adapter, IWpfTextView textView, HighlightingNavigationManager navigationManager)
            : base(adapter, textView, CmdidNextHighlightedReference, CmdidPreviousHighlightedReference)
        {
            _navigationManager = navigationManager;
        }

        protected override bool IsEnabled(VSConstants.VSStd2KCmdID commandId, ref string commandText)
        {
            // For performance reasons, don't check if we can actually move to a reference.
            return true;
        }

        protected override bool Execute(VSConstants.VSStd2KCmdID commandId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            switch (commandId)
            {
                case CmdidNextHighlightedReference:
                    _navigationManager.NavigateToNext();
                    break;
                case CmdidPreviousHighlightedReference:
                    _navigationManager.NavigateToPrevious();
                    break;
            }
            return false;
        }
    }
}