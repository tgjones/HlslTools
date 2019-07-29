// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Text;

namespace ShaderTools.CodeAnalysis.Editor.Shared.Extensions
{
    internal static class SnapshotPointExtensions
    {
        public static ITrackingPoint CreateTrackingPoint(this SnapshotPoint point, PointTrackingMode trackingMode)
        {
            return point.Snapshot.CreateTrackingPoint(point, trackingMode);
        }
    }
}
