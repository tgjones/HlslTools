using System.ComponentModel.Composition;
using System.IO;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.Editor.VisualStudio.Core.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Text
{
    [Export]
    internal sealed class VisualStudioSourceTextFactory : VisualStudioSourceTextFactoryBase
    {
        // Used for tests. A bit icky.
        public static VisualStudioSourceTextFactory Instance;

        [ImportingConstructor]
        public VisualStudioSourceTextFactory(ITextBufferFactoryService textBufferFactoryService, IContentTypeRegistryService contentTypeRegistryService)
            : base(textBufferFactoryService, contentTypeRegistryService.GetContentType(HlslConstants.ContentTypeName))
        {
        }
    }
}