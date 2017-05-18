using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;

namespace ShaderTools.CodeAnalysis.Editor.ShaderLab.Projection
{
    internal sealed class ProjectionTextViewModel : ITextViewModel
    {
        private readonly ProjectionBufferManager _projectionBufferManager;

        public ITextBuffer DataBuffer => DataModel.DataBuffer;

        public ITextDataModel DataModel { get; }

        public ITextBuffer EditBuffer { get; }

        public ITextBuffer VisualBuffer { get; }

        public PropertyCollection Properties { get; }

        public ProjectionTextViewModel(
            IProjectionBufferFactoryService projectionBufferFactoryService,
            IContentTypeRegistryService contentTypeRegistryService,
            ITextDataModel dataModel)
        {
            _projectionBufferManager = new ProjectionBufferManager(
                dataModel.DataBuffer,
                projectionBufferFactoryService,
                contentTypeRegistryService,
                ContentTypeNames.HlslContentType);

            DataModel = dataModel;
            EditBuffer = VisualBuffer = _projectionBufferManager.ViewBuffer;
            Properties = new PropertyCollection();
        }

        public SnapshotPoint GetNearestPointInVisualBuffer(SnapshotPoint editBufferPoint)
        {
            return editBufferPoint;
        }

        public SnapshotPoint GetNearestPointInVisualSnapshot(SnapshotPoint editBufferPoint, ITextSnapshot targetVisualSnapshot, PointTrackingMode trackingMode)
        {
            return editBufferPoint.TranslateTo(targetVisualSnapshot, trackingMode);
        }

        public bool IsPointInVisualBuffer(SnapshotPoint editBufferPoint, PositionAffinity affinity)
        {
            return true;
        }

        public void Dispose()
        {
            _projectionBufferManager.Dispose();
        }
    }
}