using System;
using System.Windows.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.TextManager.Interop;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Outlining
{
    internal sealed class OutliningCommandTarget : IOleCommandTarget
    {
        private readonly IOutliningManagerService _outliningManagerService;
        private IOleCommandTarget _nextCommandTarget;
        private readonly IWpfTextView _textView;

        public OutliningCommandTarget(IVsTextView adapter, IWpfTextView textView, IOutliningManagerService outliningManagerService)
        {
            _textView = textView;
            _outliningManagerService = outliningManagerService;

            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                // Add the target later to make sure it makes it in before other command handlers
                ErrorHandler.ThrowOnFailure(adapter.AddCommandFilter(this, out _nextCommandTarget));
            }, DispatcherPriority.ApplicationIdle);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == VSConstants.VSStd2K)
            {
                for (var i = 0; i < cCmds; i++)
                {
                    switch ((VSConstants.VSStd2KCmdID) prgCmds[i].cmdID)
                    {
                        case VSConstants.VSStd2KCmdID.OUTLN_START_AUTOHIDING:
                            var outliningManager = _outliningManagerService.GetOutliningManager(_textView);
                            if (outliningManager != null && !outliningManager.Enabled)
                                prgCmds[i].cmdf = (uint) (OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                    }
                }
            }

            return _nextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            var result = _nextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

            // Roslyn doesn't seem to need to do this; but without this, OutliningTagger.GetTags isn't called,
            // so outlining isn't updated after starting / stopping it.
            if (pguidCmdGroup == VSConstants.VSStd2K)
            {
                if (nCmdID == (int) VSConstants.VSStd2KCmdID.OUTLN_START_AUTOHIDING || nCmdID == (int) VSConstants.VSStd2KCmdID.OUTLN_STOP_HIDING_ALL)
                {
                    var outliningManager = _outliningManagerService.GetOutliningManager(_textView);
                    if (outliningManager != null)
                    {
                        var outliningTagger = _textView.TextBuffer.Properties.GetProperty<OutliningTagger>(typeof(OutliningTagger));
                        outliningTagger?.UpdateEnabled(outliningManager.Enabled);
                    }
                }
            }

            return result;
        }
    }
}