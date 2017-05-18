using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace ShaderTools.CodeAnalysis.Editor.ShaderLab.ContentType
{
    internal static class ContentTypeDefinitions
    {
        /// <summary>
        /// Definition of the primary ShaderLab content type.
        /// </summary>
        [Export]
        [Name(ContentTypeNames.ShaderLabContentType)]
        [BaseDefinition(ContentTypeNames.ShaderToolsContentType)]
        public static readonly ContentTypeDefinition ShaderLabContentTypeDefinition;
    }
}
