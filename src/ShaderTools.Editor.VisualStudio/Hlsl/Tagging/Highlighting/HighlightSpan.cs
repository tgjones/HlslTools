using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Highlighting
{
    internal struct HighlightSpan
    {
        public SourceFileSpan Span;
        public bool IsDefinition;

        public HighlightSpan(SourceFileSpan span, bool isDefinition)
        {
            Span = span;
            IsDefinition = isDefinition;
        }
    }
}