using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using ShaderTools.Editor.VisualStudio.Core.Util;

namespace ShaderTools.Editor.VisualStudio.Core.Tagging
{
    internal abstract class AsyncTagger<TTag> : ITagger<TTag>, IAsyncTagger
        where TTag : ITag
    {
        // Sorted based on line number.
        private List<LineBasedTagSpan> _tags = new List<LineBasedTagSpan>();
        private readonly ReaderWriterLockSlim _tagLock = new ReaderWriterLockSlim();

        public virtual async Task InvalidateTags(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            await ExceptionHelper.TryCatchCancellation(async () =>
            {
                await Task.Run(() =>
                {
                    try
                    {
                        var tagsResult = GetTags(snapshot, cancellationToken);

                        // This implicitly assumes that tags are already sorted by start index in text buffer.

                        // TODO: Really inefficient to create a new list every time.
                        // But at least it's thread-safe, and better not to potentially block the UI thread
                        // for the entire duration of updating the existing _tags.
                        var newTags = new List<LineBasedTagSpan>();
                        foreach (var tagSpan in tagsResult.Item2)
                        {
                            newTags.Add(new LineBasedTagSpan
                            {
                                StartLine = tagSpan.Span.Start.GetContainingLine().LineNumber,
                                EndLine = tagSpan.Span.End.GetContainingLine().LineNumber,
                                Span = tagSpan.Span,
                                Tag = tagSpan.Tag
                            });
                        }

                        _tagLock.EnterWriteLock();
                        try
                        {
                            _tags = newTags;
                        }
                        finally
                        {
                            _tagLock.ExitWriteLock();
                        }

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
            });
        }

        protected abstract Tuple<ITextSnapshot, List<ITagSpan<TTag>>> GetTags(ITextSnapshot snapshot, CancellationToken cancellationToken);

        public IEnumerable<ITagSpan<TTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            _tagLock.EnterReadLock();

            List<LineBasedTagSpan> tags;
            try
            {
                tags = _tags;
            }
            finally
            {
                _tagLock.ExitReadLock();
            }

            if (tags.Count == 0)
                yield break;

            foreach (var span in spans)
            foreach (var tag in GetTags(tags, span))
                yield return tag;
        }

        private static IEnumerable<ITagSpan<TTag>> GetTags(IReadOnlyList<LineBasedTagSpan> tags, SnapshotSpan targetSnapshotSpan)
        {
            var spanStartLine = targetSnapshotSpan.Start.GetContainingLine().LineNumber;
            var spanEndLine = targetSnapshotSpan.End.GetContainingLine().LineNumber;

            // Get start index.
            var startIndex = BinarySearch(tags, spanStartLine);
            var endIndex = tags.Count;

            for (var i = startIndex; i < endIndex; i++)
            {
                var tag = tags[i];

                if (tag.StartLine > spanEndLine)
                    break;

                var span = (targetSnapshotSpan.Snapshot != tag.Span.Snapshot)
                    ? tag.Span.TranslateTo(targetSnapshotSpan.Snapshot, SpanTrackingMode.EdgeExclusive)
                    : tag.Span;
                yield return new TagSpan<TTag>(span, tag.Tag);
            }
        }

        // From http://stackoverflow.com/a/594528
        private static int BinarySearch(IReadOnlyList<LineBasedTagSpan> list, int value)
        {
            int lo = 0, hi = list.Count - 1;
            while (lo < hi)
            {
                var m = (hi + lo) / 2;  // this might overflow; be careful.
                if (list[m].EndLine < value) lo = m + 1;
                else hi = m - 1;
            }
            if (list[lo].EndLine < value) lo++;
            return lo;
        }

        private void OnTagsChanged(SnapshotSpanEventArgs e)
        {
            TagsChanged?.Invoke(this, e);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private class LineBasedTagSpan
        {
            public int StartLine;
            public int EndLine;
            public SnapshotSpan Span;
            public TTag Tag;
        }
    }
}