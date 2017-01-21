using Microsoft.VisualStudio.Text.Tagging;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Highlighting
{
    internal sealed class DefinitionHighlightTag : NavigableHighlightTag
    {
        public static readonly DefinitionHighlightTag Instance = new DefinitionHighlightTag();

        private DefinitionHighlightTag()
            : base("MarkerFormatDefinition/HighlightedDefinition")
        {
        }
    }
}