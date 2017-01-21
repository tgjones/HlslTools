using Microsoft.VisualStudio.Text.Tagging;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Highlighting
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