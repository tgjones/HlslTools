using Microsoft.VisualStudio.Text.Tagging;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Highlighting
{
    internal abstract class NavigableHighlightTag : TextMarkerTag
    {
        protected NavigableHighlightTag(string type)
            : base(type)
        {

        }
    }
}
