using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;

namespace ShaderTools.CodeAnalysis.Editor.ShaderLab.Projection
{
    internal sealed class ProjectionTextViewModel : ITextViewModel
    {
        public ITextBuffer DataBuffer => DataModel.DataBuffer;

        public ITextDataModel DataModel { get; }

        public ITextBuffer EditBuffer { get; }

        public ITextBuffer VisualBuffer { get; }

        public PropertyCollection Properties { get; }

        public ProjectionTextViewModel(
            IProjectionBufferFactoryService projectionBufferFactoryService,
            ITextDataModel dataModel)
        {
            DataModel = dataModel;

            // Initially, populate projection buffer with single span covering entire data buffer.
            // We'll update this before the view actually becomes visible.
            var snapshot = dataModel.DataBuffer.CurrentSnapshot;

            var span = snapshot.CreateTrackingSpan(
                new Span(0, snapshot.Length), 
                SpanTrackingMode.EdgeExclusive);

            var projectionBuffer = projectionBufferFactoryService.CreateProjectionBuffer(
                null,
                new List<object> { span },
                ProjectionBufferOptions.None);

            EditBuffer = VisualBuffer = projectionBuffer;

            dataModel.DataBuffer.Properties.GetOrCreateSingletonProperty(() => projectionBuffer);

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
            
        }
    }
}