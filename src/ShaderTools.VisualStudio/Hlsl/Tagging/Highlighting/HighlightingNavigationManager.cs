using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace ShaderTools.VisualStudio.Hlsl.Tagging.Highlighting
{
    internal sealed class HighlightingNavigationManager
    {
        private readonly ITextView _textView;
        private readonly ITagAggregator<HighlightTag> _tagAggregator;

        public HighlightingNavigationManager(ITextView textView, ITagAggregator<HighlightTag> tagAggregator)
        {
            _textView = textView;
            _tagAggregator = tagAggregator;
        }

        public void NavigateToPrevious()
        {
            var currentTag = GetCurrentTag();
            if (currentTag == null)
                return;

            var start = currentTag.Value.Start;
            var tags = GetAllTags();
            var previousTag = tags.Reverse()
                .Where(s => s.End <= start)
                .OfType<SnapshotSpan?>()
                .FirstOrDefault();

            var tag = previousTag == null
                ? tags.Last()
                : previousTag.Value;

            SelectTag(tag);
        }

        public void NavigateToNext()
        {
            var currentTag = GetCurrentTag();
            if (currentTag == null)
                return;

            var end = currentTag.Value.End;
            var tags = GetAllTags();
            var nextTag = tags.Where(s => s.Start >= end)
                .OfType<SnapshotSpan?>()
                .FirstOrDefault();

            var tag = nextTag == null
                ? tags.First()
                : nextTag.Value;

            SelectTag(tag);
        }

        private void SelectTag(SnapshotSpan span)
        {
            _textView.Selection.Select(span, false);
            _textView.Caret.MoveTo(span.Start);
            _textView.ViewScroller.EnsureSpanVisible(span);
        }

        private SnapshotSpan? GetCurrentTag()
        {
            var snapshot = _textView.TextBuffer.CurrentSnapshot;
            var position = _textView.Caret.Position.BufferPosition.Position;
            var span = new SnapshotSpan(snapshot, position, 0);
            return GetTags(span).Cast<SnapshotSpan?>()
                .FirstOrDefault();
        }

        private IEnumerable<SnapshotSpan> GetAllTags()
        {
            var snapshot = _textView.TextBuffer.CurrentSnapshot;
            var snapshotSpan = new SnapshotSpan(snapshot, 0, snapshot.Length);
            var tags = GetTags(snapshotSpan);
            return tags;
        }

        private IEnumerable<SnapshotSpan> GetTags(SnapshotSpan snapshotSpan)
        {
            return _tagAggregator.GetTags(snapshotSpan)
                .SelectMany(m => m.Span.GetSpans(snapshotSpan.Snapshot))
                .ToImmutableArray();
        }
    }
}