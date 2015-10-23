using System;
using HlslTools.VisualStudio.Util;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace HlslTools.VisualStudio.IntelliSense.Completion
{
    internal sealed class CompletionCommandHandlerTriggers : CommandTargetBase<VSConstants.VSStd2KCmdID>
    {
        private readonly CompletionModelManager _completionModelManager;

        public CompletionCommandHandlerTriggers(IVsTextView adapter, IWpfTextView textView, CompletionModelManager completionModelManager)
            : base(adapter, textView, VSConstants.VSStd2KCmdID.SHOWMEMBERLIST, VSConstants.VSStd2KCmdID.COMPLETEWORD)
        {
            _completionModelManager = completionModelManager;
        }

        protected override bool IsEnabled(VSConstants.VSStd2KCmdID commandId, ref string commandText)
        {
            return true;
        }

        protected override bool Execute(VSConstants.VSStd2KCmdID commandId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            switch (commandId)
            {
                case VSConstants.VSStd2KCmdID.SHOWMEMBERLIST:
                    _completionModelManager.TriggerCompletion(false);
                    break;

                case VSConstants.VSStd2KCmdID.COMPLETEWORD:
                    _completionModelManager.TriggerCompletion(true);
                    break;
            }
            
            return true;
        }
    }
}