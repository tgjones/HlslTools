using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.VisualStudio.Core.Text;

namespace ShaderTools.VisualStudio.ShaderLab.Text
{
    [Export]
    internal sealed class VisualStudioSourceTextFactory : VisualStudioSourceTextFactoryBase
    {
        // Used for tests. A bit icky.
        public static VisualStudioSourceTextFactory Instance;

        [ImportingConstructor]
        public VisualStudioSourceTextFactory(ITextBufferFactoryService textBufferFactoryService, IContentTypeRegistryService contentTypeRegistryService)
            : base(textBufferFactoryService, contentTypeRegistryService.GetContentType(ShaderLabConstants.ContentTypeName))
        {
        }
    }
}