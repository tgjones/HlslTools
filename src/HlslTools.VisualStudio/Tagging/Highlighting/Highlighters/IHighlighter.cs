using System.Collections.Generic;
using HlslTools.Compilation;
using HlslTools.Syntax;

namespace HlslTools.VisualStudio.Tagging.Highlighting.Highlighters
{
    internal interface IHighlighter
    {
        IEnumerable<HighlightSpan> GetHighlights(SemanticModel semanticModel, SourceLocation position);
    }
}