using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using HlslTools.Compilation;
using HlslTools.VisualStudio.Parsing;
using HlslTools.VisualStudio.Tagging.Highlighting.Highlighters;
using HlslTools.VisualStudio.Util;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace HlslTools.VisualStudio.Tagging.Highlighting
{
    internal sealed class HighlightingTagger : AsyncTagger<HighlightTag>, IBackgroundParserSemanticModelHandler
    {
        private readonly ITextView _textView;
        private readonly ImmutableArray<IHighlighter> _highlighters;
        private readonly VisualStudioVersion _vsVersion;

        private readonly List<ITagSpan<HighlightTag>> _emptyList = new List<ITagSpan<HighlightTag>>();

        public HighlightingTagger(BackgroundParser backgroundParser, ITextView textView, ImmutableArray<IHighlighter> highlighters, IServiceProvider serviceProvider)
        {
            backgroundParser.RegisterSemanticModelHandler(BackgroundParserHandlerPriority.Low, this);

            textView.Caret.PositionChanged += OnCaretPositionChanged;

            _textView = textView;
            _highlighters = highlighters;

            var dte = serviceProvider.GetService<SDTE, DTE>();
            _vsVersion = VisualStudioVersionUtility.FromDteVersion(dte.Version);
        }

        async Task IBackgroundParserSemanticModelHandler.OnSemanticModelAvailable(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            await InvalidateTags(snapshot, cancellationToken);
        }

        private async void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            await InvalidateTags(_textView.TextSnapshot, CancellationToken.None);
        }

        protected override Tuple<ITextSnapshot, List<ITagSpan<HighlightTag>>> GetTags(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            if (snapshot != _textView.TextSnapshot)
                return Tuple.Create(snapshot, _emptyList);

            SemanticModel semanticModel;
            if (!snapshot.TryGetSemanticModel(cancellationToken, out semanticModel))
                return Tuple.Create(snapshot, _emptyList);

            var syntaxTree = semanticModel.SyntaxTree;

            var unmappedPosition = _textView.GetPosition(snapshot);
            var position = syntaxTree.MapRootFilePosition(unmappedPosition);

            var tagSpans = semanticModel.GetHighlights(position, _highlighters)
                .Select(span => (ITagSpan<HighlightTag>) new TagSpan<HighlightTag>(
                    new SnapshotSpan(snapshot, span.Span.Start, span.Span.Length),
                    new HighlightTag(_vsVersion, span.IsDefinition)));

            return Tuple.Create(snapshot, tagSpans.ToList());
        }
    }
}