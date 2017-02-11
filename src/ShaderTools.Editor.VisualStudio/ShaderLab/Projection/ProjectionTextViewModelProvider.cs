using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.Editor.VisualStudio.Core.Projection;

namespace ShaderTools.Editor.VisualStudio.ShaderLab.Projection
{
    //[Export(typeof(ITextViewModelProvider))]
    [ContentType(ShaderLabConstants.ContentTypeName)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal class ProjectionTextViewModelProvider : ITextViewModelProvider
    {
        [Import]
        public IProjectionBufferFactoryService ProjectionBufferFactory { get; set; }

        [Import]
        public IContentTypeRegistryService ContentTypeRegistryService { get; set; }

        public ITextViewModel CreateTextViewModel(ITextDataModel dataModel, ITextViewRoleSet roles)
        {
            var projectionBuffer = CreateProjectionBuffer(dataModel);
            return new ProjectionTextViewModel(dataModel, projectionBuffer);
        }

        private IProjectionBuffer CreateProjectionBuffer(ITextDataModel dataModel)
        {
            var projectionBufferManager = new ProjectionBufferManager(
                dataModel.DataBuffer,
                ProjectionBufferFactory,
                ContentTypeRegistryService,
                Hlsl.HlslConstants.ContentTypeName);

            return projectionBufferManager.ViewBuffer;
        }
    }
}