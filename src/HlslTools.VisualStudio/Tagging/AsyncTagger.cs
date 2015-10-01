using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HlslTools.VisualStudio.Parsing;
using HlslTools.VisualStudio.Util;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace HlslTools.VisualStudio.Tagging
{
    internal abstract class AsyncTagger<TTag> : ITagger<TTag>, IAsyncTagger
        where TTag : ITag
    {
        private List<LineBasedTagSpan> _tags = new List<LineBasedTagSpan>();

        public async virtual Task InvalidateTags(SnapshotSyntaxTree snapshotSyntaxTree, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                try
                {
                    var tagsResult = GetTags(snapshotSyntaxTree, cancellationToken);

                    _tags = tagsResult.Item2
                        .Select(x => new LineBasedTagSpan
                        {
                            EndLine = x.Span.End.GetContainingLine().LineNumber,
                            Span = x.Span,
                            Tag = x.Tag
                        })
                        .ToList();

                    var snapshotSpan = new SnapshotSpan(tagsResult.Item1, 0, tagsResult.Item1.Length);
                    OnTagsChanged(new SnapshotSpanEventArgs(snapshotSpan));
                }
                catch (OperationCanceledException)
                {

                }
                catch (Exception ex)
                {
                    Logger.Log("Tagging failed: " + ex);
                }
            }, cancellationToken);
        }

        protected abstract Tuple<ITextSnapshot, List<ITagSpan<TTag>>> GetTags(SnapshotSyntaxTree snapshotSyntaxTree, CancellationToken cancellationToken);

        public IEnumerable<ITagSpan<TTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            return spans.SelectMany(GetTags);
        }

        private IEnumerable<ITagSpan<TTag>> GetTags(SnapshotSpan targetSnapshotSpan)
        {
            var spanStartLine = targetSnapshotSpan.Start.GetContainingLine().LineNumber;
            var spanEndLine = targetSnapshotSpan.End.GetContainingLine().LineNumber;

            // Locations are sorted, so we can safely filter them efficiently
            return _tags
                .SkipWhile(x => x.EndLine < spanStartLine)
                .Select(x =>
                {
                    var span = (targetSnapshotSpan.Snapshot != x.Span.Snapshot)
                        ? x.Span.TranslateTo(targetSnapshotSpan.Snapshot, SpanTrackingMode.EdgeExclusive)
                        : x.Span;
                    return new { Span = span, x.Tag };
                })
                .TakeWhile(x => x.Span.Start.GetContainingLine().LineNumber <= spanEndLine)
                .Select(x => new TagSpan<TTag>(x.Span, x.Tag))
                .ToList();
        }

        private void OnTagsChanged(SnapshotSpanEventArgs e)
        {
            TagsChanged?.Invoke(this, e);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private class LineBasedTagSpan
        {
            public int EndLine;
            public SnapshotSpan Span;
            public TTag Tag;
        }
    }
}