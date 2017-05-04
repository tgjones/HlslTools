using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.Editor.VisualStudio.Core.Parsing;
using ShaderTools.Editor.VisualStudio.Core.Tagging;
using ShaderTools.Editor.VisualStudio.Hlsl.Parsing;
using ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Highlighting.Highlighters;
using ShaderTools.Editor.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Highlighting
{
    internal sealed class HighlightingTagger : AsyncTagger<NavigableHighlightTag>
    {
        private readonly ITextBuffer _textBuffer;
        private readonly ITextView _textView;
        private readonly ImmutableArray<IHighlighter> _highlighters;

        private readonly List<ITagSpan<NavigableHighlightTag>> _emptyList = new List<ITagSpan<NavigableHighlightTag>>();

        public HighlightingTagger(ITextBuffer textBuffer, BackgroundParser backgroundParser, ITextView textView, ImmutableArray<IHighlighter> highlighters)
        {
            backgroundParser.SubscribeToThrottledSemanticModelAvailable(BackgroundParserSubscriptionDelay.OnIdle,
                async x => await InvalidateTags(x.Snapshot, x.CancellationToken));

            textView.Caret.PositionChanged += OnCaretPositionChanged;

            _textBuffer = textBuffer;
            _textView = textView;
            _highlighters = highlighters;
        }

        private async void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            await InvalidateTags(_textBuffer.CurrentSnapshot, CancellationToken.None);
        }

        protected override Tuple<ITextSnapshot, List<ITagSpan<NavigableHighlightTag>>> GetTags(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            if (snapshot != _textBuffer.CurrentSnapshot)
                return Tuple.Create(snapshot, _emptyList);

            var unmappedPosition = _textView.GetPosition(snapshot);
            if (unmappedPosition == null)
                return Tuple.Create(snapshot, _emptyList);

            SemanticModel semanticModel;
            if (!snapshot.TryGetSemanticModel(cancellationToken, out semanticModel))
                return Tuple.Create(snapshot, _emptyList);

            var syntaxTree = semanticModel.SyntaxTree;
            var position = syntaxTree.MapRootFilePosition(unmappedPosition.Value);

            var tagSpans = semanticModel.GetHighlights(position, _highlighters)
                .Select(span => (ITagSpan<NavigableHighlightTag>) new TagSpan<NavigableHighlightTag>(
                    new SnapshotSpan(snapshot, span.Span.Span.Start, span.Span.Span.Length),
                    span.IsDefinition
                        ? (NavigableHighlightTag) DefinitionHighlightTag.Instance
                        : ReferenceHighlightTag.Instance));

            return Tuple.Create(snapshot, tagSpans.ToList());
        }
    }
}