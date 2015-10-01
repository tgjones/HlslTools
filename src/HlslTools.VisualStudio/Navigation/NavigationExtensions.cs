using System.Windows.Input;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;

namespace HlslTools.VisualStudio.Navigation
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
    }
}