using ShaderTools.CodeAnalysis.Editor.Shared.Tagging;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.ReferenceHighlighting
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