using System;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.BraceCompletion;

namespace HlslTools.VisualStudio.Editing.BraceCompletion
{
    internal static class BraceCompletionExtensions
    {
        // From https://github.com/dotnet/roslyn/blob/0382e3e3fc543fc483090bff3ab1eaae39dfb4d9/src/EditorFeatures/Core/Implementation/AutomaticCompletion/Extensions.cs#L106
        public static SnapshotSpan GetSessionSpan(this IBraceCompletionSession session)
        {
            var snapshot = session.SubjectBuffer.CurrentSnapshot;
            var open = session.OpeningPoint.GetPoint(snapshot);
            var close = session.ClosingPoint.GetPoint(snapshot);

            return new SnapshotSpan(open, close);
        }

        public static int GetValueInValidRange(this int value, int smallest, int largest)
        {
            return Math.Max(smallest, Math.Min(value, largest));
        }

        public static bool PositionInSnapshot(this int position, ITextSnapshot snapshot)
        {
            return position.GetValueInValidRange(0, Math.Max(0, snapshot.Length - 1)) == position;
        }
    }
}