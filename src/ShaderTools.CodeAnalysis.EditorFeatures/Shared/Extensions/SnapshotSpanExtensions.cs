// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Text;

namespace ShaderTools.CodeAnalysis.Editor.Shared.Extensions
{
    internal static class SnapshotSpanExtensions
    {
        public static ITrackingSpan CreateTrackingSpan(this SnapshotSpan snapshotSpan, SpanTrackingMode trackingMode)
        {
            return snapshotSpan.Snapshot.CreateTrackingSpan(snapshotSpan.Span, trackingMode);
        }
    }
}
