using System.IO;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace ShaderTools.Editor.VisualStudio.Core.Text
{
    internal abstract class VisualStudioSourceTextFactoryBase
    {
        private readonly ITextBufferFactoryService _textBufferFactoryService;
        private readonly IContentType _contentType;

        protected VisualStudioSourceTextFactoryBase(ITextBufferFactoryService textBufferFactoryService, IContentType contentType)
        {
            _textBufferFactoryService = textBufferFactoryService;
            _contentType = contentType;
        }

        public VisualStudioSourceText CreateSourceText(string filePath)
        {
            var textBuffer = _textBufferFactoryService.CreateTextBuffer(File.ReadAllText(filePath), _contentType);
            return new VisualStudioSourceText(textBuffer.CurrentSnapshot, filePath, false);
        }
    }
}
