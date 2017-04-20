using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.Editor.VisualStudio.Core;

namespace ShaderTools.Editor.VisualStudio.Hlsl
{
    internal static class ContentTypeDefinitions
    {
        /// <summary>
        /// Definition of the primary HLSL content type.
        /// </summary>
        [Export]
        [Name(ContentTypeNames.HlslContentType)]
        [BaseDefinition(ContentTypeNames.ShaderToolsContentType)]
        public static readonly ContentTypeDefinition HlslContentTypeDefinition;
    }
}
