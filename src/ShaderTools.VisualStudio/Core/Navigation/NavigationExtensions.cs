using System;
using System.Windows.Input;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.TextManager.Interop;
using ShaderTools.VisualStudio.Core.Util.Extensions;

namespace ShaderTools.VisualStudio.Core.Navigation
{
    internal static class NavigationExtensions
    {
        public static void NavigateTo(this IWpfTextView textView, IBufferGraphFactoryService bufferGraphFactoryService, SnapshotSpan targetSpan, SnapshotSpan targetSeek)
        {
            // show the span, then adjust if necessary to make sure the seek portion is visible
            var span = MapTo(bufferGraphFactoryService, targetSpan, textView.TextSnapshot, SpanTrackingMode.EdgeInclusive);
            textView.ViewScroller.EnsureSpanVisible(span, EnsureSpanVisibleOptions.AlwaysCenter | EnsureSpanVisibleOptions.ShowStart);

            var seek = targetSeek.Snapshot == null ? targetSpan : targetSeek;
            seek = MapTo(bufferGraphFactoryService, seek, textView.TextSnapshot, SpanTrackingMode.EdgeInclusive);
            textView.Caret.MoveTo(seek.Start);
            textView.Selection.Select(seek, false);
            textView.ViewScroller.EnsureSpanVisible(seek, EnsureSpanVisibleOptions.MinimumScroll | EnsureSpanVisibleOptions.ShowStart);
            Keyboard.Focus(textView.VisualElement);
        }

        private static SnapshotSpan MapTo(IBufferGraphFactoryService bufferGraphFactoryService, SnapshotSpan span, ITextSnapshot snapshot, SpanTrackingMode spanTrackingMode)
        {
            if (span.Snapshot.TextBuffer == snapshot.TextBuffer)
                return span.TranslateTo(snapshot, spanTrackingMode);

            var graph = bufferGraphFactoryService.CreateBufferGraph(snapshot.TextBuffer);
            var mappingSpan = graph.CreateMappingSpan(span, spanTrackingMode);
            var mapped = mappingSpan.GetSpans(snapshot);
            if (mapped.Count == 1)
                return mapped[0];

            return new SnapshotSpan(mapped[0].Start, mapped[mapped.Count - 1].End);
        }

        // TODO: Simplify to one NavigateTo implementation.
        public static void NavigateTo(this IServiceProvider serviceProvider, string fileName, int startLine, int startCol, int endRow, int endCol)
        {
            var logicalTextViewGuid = new Guid(LogicalViewID.TextView);

            IVsUIHierarchy hierarchy;
            uint itemID;
            IVsWindowFrame frame;
            var isOpened = VsShellUtilities.IsDocumentOpen(serviceProvider, fileName, logicalTextViewGuid, out hierarchy, out itemID, out frame);

            if (!isOpened)
            {
                try
                {
                    VsShellUtilities.OpenDocument(serviceProvider, fileName, logicalTextViewGuid, out hierarchy, out itemID, out frame);
                }
                catch
                {
                    return;
                }
            }

            ErrorHandler.ThrowOnFailure(frame.Show());

            var vsTextView = VsShellUtilities.GetTextView(frame);

            IVsTextLines vsTextBuffer;
            ErrorHandler.ThrowOnFailure(vsTextView.GetBuffer(out vsTextBuffer));

            var vsTextManager = serviceProvider.GetService<SVsTextManager, IVsTextManager>();
            vsTextManager.NavigateToLineAndColumn(vsTextBuffer, logicalTextViewGuid, startLine, startCol, endRow, endCol);
        }
    }
}