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
        private SortedList<int, LineBasedTagSpan> _tags = new SortedList<int, LineBasedTagSpan>();
        private readonly object _lockObject = new object();

        public virtual async Task InvalidateTags(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            await ExceptionHelper.TryCatchCancellation(async () =>
            {
                await Task.Run(() =>
                {
                    try
                    {
                        var tagsResult = GetTags(snapshot, cancellationToken);

                        // TODO: Really inefficient to create a new list every time.
                        // But at least it's thread-safe, and better not to potentially block the UI thread
                        // for the entire duration of updating the existing _tags.
                        var newTags = new SortedList<int, LineBasedTagSpan>(DuplicateKeyComparer<int>.Instance);
                        foreach (var tagSpan in tagsResult.Item2)
                        {
                            var newTag = new LineBasedTagSpan
                            {
                                StartLine = tagSpan.Span.Start.GetContainingLine().LineNumber,
                                EndLine = tagSpan.Span.End.GetContainingLine().LineNumber,
                                Span = tagSpan.Span,
                                Tag = tagSpan.Tag
                            };
                            newTags.Add(newTag.EndLine, newTag);
                        }

                        lock (_lockObject)
                            _tags = newTags;

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
            lock (_lockObject)
            {
                if (_tags.Count == 0)
                    yield break;

                foreach (var span in spans)
                foreach (var tag in GetTags(span))
                    yield return tag;
            }
        }

        private IEnumerable<ITagSpan<TTag>> GetTags(SnapshotSpan targetSnapshotSpan)
        {
            var spanStartLine = targetSnapshotSpan.Start.GetContainingLine().LineNumber;
            var spanEndLine = targetSnapshotSpan.End.GetContainingLine().LineNumber;

            // Get start index.
            var startIndex = BinarySearch(_tags.Keys, spanStartLine);
            var endIndex = _tags.Count;

            for (var i = startIndex; i < endIndex; i++)
            {
                var tag = _tags.Values[i];

                if (tag.StartLine > spanEndLine)
                    break;

                var span = (targetSnapshotSpan.Snapshot != tag.Span.Snapshot)
                    ? tag.Span.TranslateTo(targetSnapshotSpan.Snapshot, SpanTrackingMode.EdgeExclusive)
                    : tag.Span;
                yield return new TagSpan<TTag>(span, tag.Tag);
            }
        }

        // From http://stackoverflow.com/a/594528
        private static int BinarySearch<T>(IList<T> list, T value)
        {
            var comp = Comparer<T>.Default;
            int lo = 0, hi = list.Count - 1;
            while (lo < hi)
            {
                int m = (hi + lo) / 2;  // this might overflow; be careful.
                if (comp.Compare(list[m], value) < 0) lo = m + 1;
                else hi = m - 1;
            }
            if (comp.Compare(list[lo], value) < 0) lo++;
            return lo;
        }

        /// <summary>
        /// Comparer for comparing two keys, handling equality as beeing greater
        /// Use this Comparer e.g. with SortedLists or SortedDictionaries, that don't allow duplicate keys
        /// From http://stackoverflow.com/a/21886340
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        private sealed class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
        {
            public static IComparer<TKey> Instance { get; } = new DuplicateKeyComparer<TKey>();

            public int Compare(TKey x, TKey y)
            {
                int result = x.CompareTo(y);

                if (result == 0)
                    return 1;   // Handle equality as beeing greater
                else
                    return result;
            }
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