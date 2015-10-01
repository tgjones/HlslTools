using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HlslTools.VisualStudio.Parsing;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace HlslTools.VisualStudio.Tagging.BraceMatching
{
    internal sealed class BraceMatchingTagger : AsyncTagger<ITextMarkerTag>, IBackgroundParserSyntaxTreeHandler
    {
        private readonly ITextView _textView;
        private readonly BraceMatcher _braceMatcher;

        private readonly List<ITagSpan<ITextMarkerTag>> _emptyList = new List<ITagSpan<ITextMarkerTag>>();
        private readonly ITextMarkerTag _tag = new TextMarkerTag("bracehighlight");

        private SnapshotSyntaxTree _latestSnapshotSyntaxTree;

        public BraceMatchingTagger(BackgroundParser backgroundParser, ITextView textView, BraceMatcher braceMatcher)
        {
            backgroundParser.RegisterSyntaxTreeHandler(BackgroundParserHandlerPriority.Medium, this);

            textView.Caret.PositionChanged += OnCaretPositionChanged;

            _textView = textView;
            _braceMatcher = braceMatcher;
        }

        async Task IBackgroundParserSyntaxTreeHandler.OnSyntaxTreeAvailable(SnapshotSyntaxTree snapshotSyntaxTree, CancellationToken cancellationToken)
        {
            await InvalidateTags(snapshotSyntaxTree, cancellationToken);
        }

        public override Task InvalidateTags(SnapshotSyntaxTree snapshotSyntaxTree, CancellationToken cancellationToken)
        {
            _latestSnapshotSyntaxTree = snapshotSyntaxTree;
            return base.InvalidateTags(snapshotSyntaxTree, cancellationToken);
        }

        private async void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            var latest = _latestSnapshotSyntaxTree;
            if (latest != null)
                await InvalidateTags(latest, CancellationToken.None);
        }

        protected override Tuple<ITextSnapshot, List<ITagSpan<ITextMarkerTag>>> GetTags(SnapshotSyntaxTree snapshotSyntaxTree, CancellationToken cancellationToken)
        {
            var snapshot = snapshotSyntaxTree.Snapshot;

            if (snapshot != _textView.TextSnapshot)
                return Tuple.Create(snapshot, _emptyList);

            var syntaxTree = snapshotSyntaxTree.SyntaxTree;

            var unmappedPosition = _textView.GetPosition(snapshot);
            var position =  syntaxTree.MapRootFilePosition(unmappedPosition);

            var result = _braceMatcher.MatchBraces(syntaxTree, position);
            if (!result.IsValid)
                return Tuple.Create(snapshot, _emptyList);

            var leftTag = new TagSpan<ITextMarkerTag>(new SnapshotSpan(snapshot, result.Left.Start, result.Left.Length), _tag);
            var rightTag = new TagSpan<ITextMarkerTag>(new SnapshotSpan(snapshot, result.Right.Start, result.Right.Length), _tag);

            return Tuple.Create(snapshot, new List<ITagSpan<ITextMarkerTag>> { leftTag, rightTag });
        }
    }
}