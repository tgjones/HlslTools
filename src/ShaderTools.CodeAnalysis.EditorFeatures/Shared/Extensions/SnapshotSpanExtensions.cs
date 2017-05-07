// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Text;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Text.Shared.Extensions;

namespace ShaderTools.CodeAnalysis.Editor.Shared.Extensions
{
    internal static class SnapshotSpanExtensions
    {
        public static ITrackingSpan CreateTrackingSpan(this SnapshotSpan snapshotSpan, SpanTrackingMode trackingMode)
        {
            return snapshotSpan.Snapshot.CreateTrackingSpan(snapshotSpan.Span, trackingMode);
        }

        public static void GetLinesAndColumns(
            this SnapshotSpan snapshotSpan,
            out int startLineNumber,
            out int startColumnIndex,
            out int endLineNumber,
            out int endColumnIndex)
        {
            snapshotSpan.Snapshot.GetLineAndColumn(snapshotSpan.Span.Start, out startLineNumber, out startColumnIndex);
            snapshotSpan.Snapshot.GetLineAndColumn(snapshotSpan.Span.End, out endLineNumber, out endColumnIndex);
        }

        public static bool IntersectsWith(this SnapshotSpan snapshotSpan, TextSpan textSpan)
        {
            return snapshotSpan.IntersectsWith(textSpan.ToSpan());
        }

        public static bool IntersectsWith(this SnapshotSpan snapshotSpan, int position)
        {
            return snapshotSpan.Span.IntersectsWith(position);
        }
    }
}
