// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Symbols.Markup;
using ShaderTools.Utilities.Diagnostics;

namespace ShaderTools.CodeAnalysis.Shared.Extensions
{
    internal static class StringExtensions
    {
        public static int? GetFirstNonWhitespaceOffset(this string line)
        {
            Contract.ThrowIfNull(line);

            for (int i = 0; i < line.Length; i++)
            {
                if (!char.IsWhiteSpace(line[i]))
                {
                    return i;
                }
            }

            return null;
        }

        public static int ConvertTabToSpace(this string textSnippet, int tabSize, int initialColumn, int endPosition)
        {
            Contract.Requires(tabSize > 0);
            Contract.Requires(endPosition >= 0 && endPosition <= textSnippet.Length);

            int column = initialColumn;

            // now this will calculate indentation regardless of actual content on the buffer except TAB
            for (int i = 0; i < endPosition; i++)
            {
                if (textSnippet[i] == '\t')
                {
                    column += tabSize - column % tabSize;
                }
                else
                {
                    column++;
                }
            }

            return column - initialColumn;
        }

        public static bool ContainsLineBreak(this string text)
        {
            foreach (char ch in text)
            {
                if (ch == '\n' || ch == '\r')
                {
                    return true;
                }
            }

            return false;
        }

        public static int GetColumnOfFirstNonWhitespaceCharacterOrEndOfLine(this string line, int tabSize)
        {
            var firstNonWhitespaceChar = line.GetFirstNonWhitespaceOffset();

            if (firstNonWhitespaceChar.HasValue)
            {
                return line.GetColumnFromLineOffset(firstNonWhitespaceChar.Value, tabSize);
            }
            else
            {
                // It's all whitespace, so go to the end
                return line.GetColumnFromLineOffset(line.Length, tabSize);
            }
        }

        public static int GetColumnFromLineOffset(this string line, int endPosition, int tabSize)
        {
            Contract.ThrowIfNull(line);
            Contract.ThrowIfFalse(0 <= endPosition && endPosition <= line.Length);
            Contract.ThrowIfFalse(tabSize > 0);

            return ConvertTabToSpace(line, tabSize, 0, endPosition);
        }

        public static int GetLineOffsetFromColumn(this string line, int column, int tabSize)
        {
            Contract.ThrowIfNull(line);
            Contract.ThrowIfFalse(column >= 0);
            Contract.ThrowIfFalse(tabSize > 0);

            var currentColumn = 0;

            for (int i = 0; i < line.Length; i++)
            {
                if (currentColumn >= column)
                {
                    return i;
                }

                if (line[i] == '\t')
                {
                    currentColumn += tabSize - (currentColumn % tabSize);
                }
                else
                {
                    currentColumn++;
                }
            }

            // We're asking for a column past the end of the line, so just go to the end.
            return line.Length;
        }

        public static ImmutableArray<SymbolMarkupToken> ToSymbolMarkupTokens(this string text)
        {
            return ImmutableArray.Create(new SymbolMarkupToken(SymbolMarkupKind.PlainText, text));
        }
    }
}
