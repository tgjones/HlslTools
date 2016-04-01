using System.Collections.Generic;
using System.Linq;
using HlslTools.Compilation;
using HlslTools.Syntax;
using HlslTools.Text;
using HlslTools.VisualStudio.Tagging.Highlighting.Highlighters;

namespace HlslTools.VisualStudio.Tagging.Highlighting
{
    internal static class HighlightingExtensions
    {
        public static IEnumerable<HighlightSpan> GetHighlights(this SemanticModel semanticModel, SourceLocation position, IEnumerable<IHighlighter> highlighters)
        {
            var result = new List<HighlightSpan>();

            foreach (var highlighter in highlighters)
            {
                result.AddRange(highlighter
                    .GetHighlights(semanticModel, position)
                    .Where(x => x.Span.IsInRootFile));
            }

            return result;
        }
    }
}