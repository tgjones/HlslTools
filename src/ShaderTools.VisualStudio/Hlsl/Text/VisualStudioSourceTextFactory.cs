using System.ComponentModel.Composition;
using System.IO;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.VisualStudio.Core.Text;

namespace ShaderTools.VisualStudio.Hlsl.Text
{
    [Export]
    internal sealed class VisualStudioSourceTextFactory
    {
        // Used for tests. A bit icky.
        public static VisualStudioSourceTextFactory Instance;

        private readonly ITextBufferFactoryService _textBufferFactoryService;
        private readonly IContentType _contentType;

        [ImportingConstructor]
        public VisualStudioSourceTextFactory(ITextBufferFactoryService textBufferFactoryService, IContentTypeRegistryService contentTypeRegistryService)
        {
            _textBufferFactoryService = textBufferFactoryService;
            _contentType = contentTypeRegistryService.GetContentType(HlslConstants.ContentTypeName);
        }

        public VisualStudioSourceText CreateSourceText(string filePath)
        {
            var textBuffer = _textBufferFactoryService.CreateTextBuffer(File.ReadAllText(filePath), _contentType);
            return new VisualStudioSourceText(textBuffer.CurrentSnapshot, filePath);
        }
    }
}