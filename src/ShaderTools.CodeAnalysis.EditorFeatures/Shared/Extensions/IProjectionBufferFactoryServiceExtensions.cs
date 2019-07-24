// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.CodeAnalysis.Shared.Extensions;
using ShaderTools.CodeAnalysis.Text.Shared.Extensions;
using ShaderTools.Utilities.Collections;

namespace ShaderTools.CodeAnalysis.Editor.Shared.Extensions
{
    internal static class IProjectionBufferFactoryServiceExtensions
    {
        public const string ShaderToolsPreviewContentType = nameof(ShaderToolsPreviewContentType);

        /// <summary>
        /// Hack to get view taggers working on our preview surfaces.  We need to define
        /// both projection and text in order for this to work.  Talk to JasonMal for he is the only
        /// one who understands this.
        /// </summary>
        [Export]
        [Name(ShaderToolsPreviewContentType)]
        [BaseDefinition("text")]
        [BaseDefinition("projection")]
        public static readonly ContentTypeDefinition ShaderToolsPreviewContentTypeDefinition;

        private static int DetermineIndentationColumn(
            IEditorOptions editorOptions,
            IEnumerable<SnapshotSpan> spans)
        {
            int? indentationColumn = null;
            foreach (var span in spans)
            {
                var snapshot = span.Snapshot;
                var startLineNumber = snapshot.GetLineNumberFromPosition(span.Start);
                var endLineNumber = snapshot.GetLineNumberFromPosition(span.End);

                // If the span starts after the first non-whitespace of the first line, we'll
                // exclude that line to avoid throwing off the calculation. Otherwise, the
                // incorrect indentation will be returned for lambda cases like so:
                //
                // void M()
                // {
                //     Func<int> f = () =>
                //         {
                //             return 1;
                //         };
                // }
                //
                // Without throwing out the first line in the example above, the indentation column
                // used will be 4, rather than 8.
                var startLineFirstNonWhitespace = snapshot.GetLineFromLineNumber(startLineNumber).GetFirstNonWhitespacePosition();
                if (startLineFirstNonWhitespace.HasValue && startLineFirstNonWhitespace.Value < span.Start)
                {
                    startLineNumber++;
                }

                for (var lineNumber = startLineNumber; lineNumber <= endLineNumber; lineNumber++)
                {
                    var line = snapshot.GetLineFromLineNumber(lineNumber);
                    if (string.IsNullOrWhiteSpace(line.GetText()))
                    {
                        continue;
                    }

                    indentationColumn = indentationColumn.HasValue
                        ? Math.Min(indentationColumn.Value, line.GetColumnOfFirstNonWhitespaceCharacterOrEndOfLine(editorOptions))
                        : line.GetColumnOfFirstNonWhitespaceCharacterOrEndOfLine(editorOptions);
                }
            }

            return indentationColumn ?? 0;
        }

        public static IProjectionBuffer CreateProjectionBufferWithoutIndentation(
            this IProjectionBufferFactoryService factoryService,
            IContentTypeRegistryService registryService,
            IEditorOptions editorOptions,
            ITextSnapshot snapshot,
            string separator,
            params LineSpan[] exposedLineSpans)
        {
            return factoryService.CreateProjectionBufferWithoutIndentation(
                registryService,
                editorOptions,
                snapshot,
                separator,
                suffixOpt: null,
                exposedLineSpans: exposedLineSpans);
        }

        public static IProjectionBuffer CreateProjectionBufferWithoutIndentation(
            this IProjectionBufferFactoryService factoryService,
            IContentTypeRegistryService registryService,
            IEditorOptions editorOptions,
            ITextSnapshot snapshot,
            string separator,
            object suffixOpt,
            params LineSpan[] exposedLineSpans)
        {
            return CreateProjectionBuffer(
                factoryService,
                registryService,
                editorOptions,
                snapshot,
                separator,
                suffixOpt,
                trim: true,
                exposedLineSpans: exposedLineSpans);
        }

        private static IProjectionBuffer CreateProjectionBuffer(
            IProjectionBufferFactoryService factoryService,
            IContentTypeRegistryService registryService,
            IEditorOptions editorOptions,
            ITextSnapshot snapshot,
            string separator,
            object suffixOpt,
            bool trim,
            params LineSpan[] exposedLineSpans)
        {
            var spans = new List<object>();
            if (exposedLineSpans.Length > 0)
            {
                if (exposedLineSpans[0].Start > 0 && !string.IsNullOrEmpty(separator))
                {
                    spans.Add(separator);
                    spans.Add(editorOptions.GetNewLineCharacter());
                }

                var snapshotSpanRanges = CreateSnapshotSpanRanges(snapshot, exposedLineSpans);
                var indentColumn = trim
                    ? DetermineIndentationColumn(editorOptions, snapshotSpanRanges.Flatten())
                    : 0;

                foreach (var snapshotSpanRange in snapshotSpanRanges)
                {
                    foreach (var snapshotSpan in snapshotSpanRange)
                    {
                        var line = snapshotSpan.Snapshot.GetLineFromPosition(snapshotSpan.Start);
                        var indentPosition = line.GetLineOffsetFromColumn(indentColumn, editorOptions) + line.Start;
                        var mappedSpan = new SnapshotSpan(snapshotSpan.Snapshot,
                            Span.FromBounds(indentPosition, snapshotSpan.End));

                        var trackingSpan = mappedSpan.CreateTrackingSpan(SpanTrackingMode.EdgeExclusive);

                        spans.Add(trackingSpan);

                        // Add a newline between every line.
                        if (snapshotSpan != snapshotSpanRange.Last())
                        {
                            spans.Add(editorOptions.GetNewLineCharacter());
                        }
                    }

                    // Add a separator between every set of lines.
                    if (snapshotSpanRange != snapshotSpanRanges.Last())
                    {
                        spans.Add(editorOptions.GetNewLineCharacter());
                        spans.Add(separator);
                        spans.Add(editorOptions.GetNewLineCharacter());
                    }
                }

                if (snapshot.GetLineNumberFromPosition(snapshotSpanRanges.Last().Last().End) < snapshot.LineCount - 1)
                {
                    spans.Add(editorOptions.GetNewLineCharacter());
                    spans.Add(separator);
                }
            }

            if (suffixOpt != null)
            {
                if (spans.Count >= 0)
                {
                    if (!separator.Equals(spans.Last()))
                    {
                        spans.Add(editorOptions.GetNewLineCharacter());
                        spans.Add(separator);
                    }

                    spans.Add(editorOptions.GetNewLineCharacter());
                }

                spans.Add(suffixOpt);
            }

            return factoryService.CreateProjectionBuffer(
                projectionEditResolver: null,
                sourceSpans: spans,
                options: ProjectionBufferOptions.None,
                contentType: registryService.GetContentType(ShaderToolsPreviewContentType));
        }

        private static IList<IList<SnapshotSpan>> CreateSnapshotSpanRanges(ITextSnapshot snapshot, LineSpan[] exposedLineSpans)
        {
            var result = new List<IList<SnapshotSpan>>();
            foreach (var lineSpan in exposedLineSpans)
            {
                var snapshotSpans = CreateSnapshotSpans(snapshot, lineSpan);
                if (snapshotSpans.Count > 0)
                {
                    result.Add(snapshotSpans);
                }
            }

            return result;
        }

        private static IList<SnapshotSpan> CreateSnapshotSpans(ITextSnapshot snapshot, LineSpan lineSpan)
        {
            var result = new List<SnapshotSpan>();
            for (int i = lineSpan.Start; i < lineSpan.End; i++)
            {
                var line = snapshot.GetLineFromLineNumber(i);
                result.Add(line.Extent);
            }

            return result;
        }
    }
}
