using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using HlslTools.Compilation;
using HlslTools.Syntax;
using HlslTools.VisualStudio.SymbolSearch;

namespace HlslTools.VisualStudio.Tagging.Highlighting.Highlighters
{
    [Export(typeof(IHighlighter))]
    internal sealed class SymbolReferenceHighlighter : IHighlighter
    {
        public IEnumerable<HighlightSpan> GetHighlights(SemanticModel semanticModel, SourceLocation position)
        {
            var symbolAtPosition = semanticModel.FindSymbol(position);
            if (symbolAtPosition == null)
                return Enumerable.Empty<HighlightSpan>();

            return semanticModel.FindUsages(symbolAtPosition.Value.Symbol)
                .Select(s => new HighlightSpan(s.Span, s.Kind == SymbolSpanKind.Definition));
        }
    }
}