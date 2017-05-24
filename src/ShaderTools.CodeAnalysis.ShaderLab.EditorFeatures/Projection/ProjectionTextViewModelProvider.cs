using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;

namespace ShaderTools.CodeAnalysis.Editor.ShaderLab.Projection
{
    [Export(typeof(ITextViewModelProvider))]
    [ContentType(ContentTypeNames.ShaderLabContentType)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class ProjectionTextViewModelProvider : ITextViewModelProvider
    {
        private readonly IProjectionBufferFactoryService _projectionBufferFactoryService;
        private readonly IContentTypeRegistryService _contentTypeRegistryService;

        [ImportingConstructor]
        public ProjectionTextViewModelProvider(
            IProjectionBufferFactoryService projectionBufferFactoryService,
            IContentTypeRegistryService contentTypeRegistryService)
        {
            _projectionBufferFactoryService = projectionBufferFactoryService;
            _contentTypeRegistryService = contentTypeRegistryService;
        }

        public ITextViewModel CreateTextViewModel(ITextDataModel dataModel, ITextViewRoleSet roles)
        {
            return new ProjectionTextViewModel(
                _projectionBufferFactoryService,
                _contentTypeRegistryService,
                dataModel);
        }
    }
}