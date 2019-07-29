// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using ShaderTools.Utilities.Diagnostics;

namespace ShaderTools.CodeAnalysis.Text.Shared.Extensions
{
    internal static partial class ITextSnapshotExtensions
    {
        public static SnapshotSpan GetSpanFromBounds(this ITextSnapshot snapshot, int start, int end)
            => new SnapshotSpan(snapshot, Span.FromBounds(start, end));

        public static SnapshotSpan GetSpan(this ITextSnapshot snapshot, Span span)
            => new SnapshotSpan(snapshot, span);

        public static ITagSpan<TTag> GetTagSpan<TTag>(this ITextSnapshot snapshot, Span span, TTag tag)
            where TTag : ITag
        {
            return new TagSpan<TTag>(new SnapshotSpan(snapshot, span), tag);
        }

        public static SnapshotSpan GetFullSpan(this ITextSnapshot snapshot)
        {
            Contract.ThrowIfNull(snapshot);

            return new SnapshotSpan(snapshot, new Span(0, snapshot.Length));
        }
    }
}