using System.Collections.Immutable;

namespace ShaderTools.CodeAnalysis.ReferenceHighlighting
{
    internal struct DocumentHighlights
    {
        public Document Document { get; }
        public ImmutableArray<HighlightSpan> HighlightSpans { get; }

        public DocumentHighlights(Document document, ImmutableArray<HighlightSpan> highlightSpans)
        {
            Document = document;
            HighlightSpans = highlightSpans;
        }
    }
}
