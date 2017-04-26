using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Highlighting.Highlighters
{
    internal interface IHighlighter
    {
        IEnumerable<HighlightSpan> GetHighlights(SemanticModel semanticModel, SourceLocation position);
    }
}