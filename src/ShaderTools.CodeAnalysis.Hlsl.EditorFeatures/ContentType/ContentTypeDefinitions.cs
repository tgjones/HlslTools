using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace ShaderTools.CodeAnalysis.Editor.Hlsl.ContentType
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
