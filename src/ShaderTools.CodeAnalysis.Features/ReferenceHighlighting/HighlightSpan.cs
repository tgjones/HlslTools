using Microsoft.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.ReferenceHighlighting
{
    internal struct HighlightSpan
    {
        public TextSpan TextSpan;
        public HighlightSpanKind Kind { get; }

        public HighlightSpan(TextSpan textSpan, HighlightSpanKind kind)
        {
            TextSpan = textSpan;
            Kind = kind;
        }
    }
}