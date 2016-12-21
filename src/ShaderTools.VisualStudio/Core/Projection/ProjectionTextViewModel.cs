using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;

namespace ShaderTools.VisualStudio.Core.Projection
{
    internal sealed class ProjectionTextViewModel : ITextViewModel
    {
        public ITextBuffer DataBuffer => DataModel.DataBuffer;

        public ITextDataModel DataModel { get; }

        public ITextBuffer EditBuffer { get; }

        public ITextBuffer VisualBuffer { get; }

        public PropertyCollection Properties { get; }

        public void Dispose()
        {
        }

        public ProjectionTextViewModel(ITextDataModel dataModel, IProjectionBuffer projectionBuffer)
        {
            DataModel = dataModel;
            EditBuffer = VisualBuffer = projectionBuffer;
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
    }
}