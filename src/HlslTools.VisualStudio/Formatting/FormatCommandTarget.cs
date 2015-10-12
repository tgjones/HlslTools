using System;
using System.Windows.Threading;
using HlslTools.VisualStudio.Options;
using HlslTools.VisualStudio.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using TextSpan = HlslTools.Text.TextSpan;

namespace HlslTools.VisualStudio.Formatting
{
    internal sealed class FormatCommandTarget : IOleCommandTarget
    {
        private IOleCommandTarget _nextCommandTarget;
        private readonly IWpfTextView _textView;
        private readonly IOptionsService _optionsService;
        private readonly VisualStudioSourceTextFactory _sourceTextFactory;

        public FormatCommandTarget(IVsTextView adapter, IWpfTextView textView, IOptionsService optionsService, VisualStudioSourceTextFactory sourceTextFactory)
        {
            _textView = textView;
            _optionsService = optionsService;
            _sourceTextFactory = sourceTextFactory;

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
                        case VSConstants.VSStd2KCmdID.FORMATDOCUMENT:
                        case VSConstants.VSStd2KCmdID.FORMATSELECTION:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                    }
                }
            }

            return _nextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == VSConstants.VSStd2K)
            {
                switch ((VSConstants.VSStd2KCmdID) nCmdID)
                {
                    case VSConstants.VSStd2KCmdID.FORMATDOCUMENT:
                        FormatSpan(0, _textView.TextBuffer.CurrentSnapshot.Length);
                        return VSConstants.S_OK;

                    case VSConstants.VSStd2KCmdID.FORMATSELECTION:
                        FormatSelection();
                        return VSConstants.S_OK;

                    case VSConstants.VSStd2KCmdID.TYPECHAR:
                        return TypeChar(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                }
            }
            else if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97)
            {
                switch ((VSConstants.VSStd97CmdID)nCmdID)
                {
                    case VSConstants.VSStd97CmdID.Paste:
                        return Paste(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                }
            }

            return _nextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private void FormatSelection()
        {
            var start = _textView.Selection.Start.Position.Position;
            var length = _textView.Selection.End.Position.Position - start;

            FormatSpan(start, length);
        }

        private void FormatSpan(int start, int length)
        {
            _textView.TextBuffer.Format(new TextSpan(_textView.TextSnapshot.ToSourceText(), start, length), _optionsService, _sourceTextFactory);
        }

        private int Paste(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            var currentSnapshot = _textView.Caret.Position.BufferPosition.Snapshot;

            var hr = _nextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

            if (ErrorHandler.Failed(hr))
                return hr;

            FormatAfterPaste(currentSnapshot);
            return VSConstants.S_OK;
        }

        // From https://github.com/Microsoft/nodejstools/blob/master/Nodejs/Product/Nodejs/EditFilter.cs#L901
        private void FormatAfterPaste(ITextSnapshot curVersion)
        {
            if (!_optionsService.GeneralOptions.AutomaticallyFormatOnPaste)
                return;

            // calculate the range for the paste...
            var afterVersion = curVersion.TextBuffer.CurrentSnapshot;
            int start = afterVersion.Length, end = 0;
            for (var version = curVersion.Version;
                version != afterVersion.Version;
                version = version.Next)
            {
                foreach (var change in version.Changes)
                {
                    var oldStart = version
                        .CreateTrackingPoint(change.OldPosition, PointTrackingMode.Negative)
                        .GetPosition(afterVersion.Version);

                    start = Math.Min(oldStart, start);

                    var newEnd = version.Next
                        .CreateTrackingPoint(change.NewSpan.End, PointTrackingMode.Positive)
                        .GetPosition(afterVersion.Version);

                    end = Math.Max(newEnd, end);
                }
            }

            if (start < end)
                FormatSpan(start, end - start);
        }

        private int TypeChar(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            var ch = (char)(ushort)System.Runtime.InteropServices.Marshal.GetObjectForNativeVariant(pvaIn);
            var res = _nextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

            switch (ch)
            {
                case '}':
                case ';':
                    _textView.FormatAfterTyping(ch, _optionsService, _sourceTextFactory);
                    break;
            }

            return res;
        }
    }
}