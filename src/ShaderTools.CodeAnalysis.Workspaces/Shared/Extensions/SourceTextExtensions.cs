// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.Utilities.Diagnostics;

namespace ShaderTools.CodeAnalysis.Shared.Extensions
{
    internal static class SourceTextExtensions
    {
        public static void GetLineAndOffset(this SourceText text, int position, out int lineNumber, out int offset)
        {
            var line = text.Lines.GetLineFromPosition(position);

            lineNumber = line.LineNumber;
            offset = position - line.Start;
        }

        public static void GetLinesAndOffsets(
            this SourceText text,
            TextSpan textSpan,
            out int startLineNumber,
            out int startOffset,
            out int endLineNumber,
            out int endOffset)
        {
            text.GetLineAndOffset(textSpan.Start, out startLineNumber, out startOffset);
            text.GetLineAndOffset(textSpan.End, out endLineNumber, out endOffset);
        }

        public static int IndexOf(this SourceText text, string value, int startIndex, bool caseSensitive)
        {
            var length = text.Length - value.Length;
            var normalized = caseSensitive ? value : CaseInsensitiveComparison.ToLower(value);

            for (var i = startIndex; i <= length; i++)
            {
                var match = true;
                for (var j = 0; j < normalized.Length; j++)
                {
                    // just use indexer of source text. perf of indexer depends on actual implementation of SourceText.
                    // * all of our implementation at editor layer should provide either O(1) or O(logn).
                    //
                    // only one implementation we have that could have bad indexer perf is CompositeText with heavily modified text
                    // at compiler layer but I believe that being used in find all reference will be very rare if not none.
                    if (!Match(normalized[j], text[i + j], caseSensitive))
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return i;
                }
            }

            return -1;
        }

        public static int LastIndexOf(this SourceText text, string value, int startIndex, bool caseSensitive)
        {
            var normalized = caseSensitive ? value : CaseInsensitiveComparison.ToLower(value);
            startIndex = startIndex + normalized.Length > text.Length
                ? text.Length - normalized.Length
                : startIndex;

            for (var i = startIndex; i >= 0; i--)
            {
                var match = true;
                for (var j = 0; j < normalized.Length; j++)
                {
                    // just use indexer of source text. perf of indexer depends on actual implementation of SourceText.
                    // * all of our implementation at editor layer should provide either O(1) or O(logn).
                    //
                    // only one implementation we have that could have bad indexer perf is CompositeText with heavily modified text
                    // at compiler layer but I believe that being used in find all reference will be very rare if not none.
                    if (!Match(normalized[j], text[i + j], caseSensitive))
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return i;
                }
            }

            return -1;
        }

        private static bool Match(char normalizedLeft, char right, bool caseSensitive)
        {
            return caseSensitive ? normalizedLeft == right : normalizedLeft == CaseInsensitiveComparison.ToLower(right);
        }
    }
}
