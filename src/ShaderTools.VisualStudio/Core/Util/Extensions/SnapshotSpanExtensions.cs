using Microsoft.VisualStudio.Text;

namespace ShaderTools.VisualStudio.Core.Util.Extensions
{
    internal static class SnapshotSpanExtensions
    {
        public static ITrackingSpan CreateTrackingSpan(this SnapshotSpan snapshotSpan, SpanTrackingMode trackingMode)
        {
            return snapshotSpan.Snapshot.CreateTrackingSpan(snapshotSpan.Span, trackingMode);
        }

        public static bool IntersectsWith(this SnapshotSpan snapshotSpan, int position)
        {
            return snapshotSpan.Span.IntersectsWith(position);
        }
    }
}