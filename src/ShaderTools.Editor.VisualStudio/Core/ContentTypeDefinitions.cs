using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace ShaderTools.Editor.VisualStudio.Core
{
    internal static class ContentTypeDefinitions
    {
        /// <summary>
        /// Definition of a content type that is a base definition for all content types supported by Shader Tools.
        /// </summary>
        [Export]
        [Name(ContentTypeNames.ShaderToolsContentType)]
        [BaseDefinition("code")]
        public static readonly ContentTypeDefinition ShaderToolsContentTypeDefinition;
    }
}
