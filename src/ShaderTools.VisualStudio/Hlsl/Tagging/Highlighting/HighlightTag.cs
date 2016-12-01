using Microsoft.VisualStudio.Text.Tagging;
using ShaderTools.VisualStudio.Core.Util;

namespace ShaderTools.VisualStudio.Hlsl.Tagging.Highlighting
{
    internal sealed class HighlightTag : TextMarkerTag
    {
        private static string GetTagType(VisualStudioVersion version, bool isDefinition)
        {
            if (version == VisualStudioVersion.Vs2015 && isDefinition)
                return "MarkerFormatDefinition/HighlightedDefinition";
            return "MarkerFormatDefinition/HighlightedReference";
        }

        public HighlightTag(VisualStudioVersion vsVersion, bool isDefinition)
            : base(GetTagType(vsVersion, isDefinition))
        {
        }
    }
}