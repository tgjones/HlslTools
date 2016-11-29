using System.ComponentModel.Composition.Hosting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.VisualStudio.Hlsl;

namespace ShaderTools.VisualStudio.Tests.Hlsl.Support
{
    internal static class TextBufferUtility
    {
        public static ITextBuffer CreateTextBuffer(CompositionContainer container, string text)
        {
            var contentTypeRegistry = container.GetExportedValue<IContentTypeRegistryService>();
            var contentType = contentTypeRegistry.GetContentType(HlslConstants.ContentTypeName)
                ?? contentTypeRegistry.AddContentType(HlslConstants.ContentTypeName, null);

            var textBufferFactory = container.GetExportedValue<ITextBufferFactoryService>();
            var textBuffer = textBufferFactory.CreateTextBuffer(text, contentType);

            return textBuffer;
        }
    }
}