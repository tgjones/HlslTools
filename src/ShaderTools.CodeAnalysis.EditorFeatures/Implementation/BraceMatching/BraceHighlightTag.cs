using Microsoft.VisualStudio.Text.Tagging;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.BraceMatching
{
    internal class BraceHighlightTag : TextMarkerTag
    {
        public static readonly BraceHighlightTag Tag = new BraceHighlightTag();

        private BraceHighlightTag()
            : base("brace matching")
        {
        }
    }
}
