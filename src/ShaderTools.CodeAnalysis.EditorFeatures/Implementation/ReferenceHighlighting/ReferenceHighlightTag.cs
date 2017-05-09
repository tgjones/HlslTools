using ShaderTools.CodeAnalysis.Editor.Shared.Tagging;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.ReferenceHighlighting
{
    internal sealed class ReferenceHighlightTag : NavigableHighlightTag
    {
        public static readonly ReferenceHighlightTag Instance = new ReferenceHighlightTag();

        private ReferenceHighlightTag()
            : base("MarkerFormatDefinition/HighlightedReference")
        {
        }
    }
}