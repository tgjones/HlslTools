using System;

namespace ShaderTools.VisualStudio.Hlsl.Util.Extensions
{
    // https://github.com/dotnet/roslyn/blob/master/src/Workspaces/Core/Portable/Shared/Extensions/StringExtensions.cs
    internal static class StringExtensions
    {
        public static int? GetFirstNonWhitespaceOffset(this string line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                if (!char.IsWhiteSpace(line[i]))
                {
                    return i;
                }
            }

            return null;
        }

        public static string GetLeadingWhitespace(this string lineText)
        {
            var firstOffset = lineText.GetFirstNonWhitespaceOffset();

            return firstOffset.HasValue
                ? lineText.Substring(0, firstOffset.Value)
                : lineText;
        }

        public static int GetTextColumn(this string text, int tabSize, int initialColumn)
        {
            var lineText = text.GetLastLineText();
            if (text != lineText)
            {
                return lineText.GetColumnFromLineOffset(lineText.Length, tabSize);
            }

            return text.ConvertTabToSpace(tabSize, initialColumn, text.Length) + initialColumn;
        }

        public static int ConvertTabToSpace(this string textSnippet, int tabSize, int initialColumn, int endPosition)
        {
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

        public static int IndexOf(this string text, Func<char, bool> predicate)
        {
            if (text == null)
            {
                return -1;
            }

            for (int i = 0; i < text.Length; i++)
            {
                if (predicate(text[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        public static string GetFirstLineText(this string text)
        {
            var lineBreak = text.IndexOf('\n');
            if (lineBreak < 0)
            {
                return text;
            }

            return text.Substring(0, lineBreak + 1);
        }

        public static string GetLastLineText(this string text)
        {
            var lineBreak = text.LastIndexOf('\n');
            if (lineBreak < 0)
            {
                return text;
            }

            return text.Substring(lineBreak + 1);
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

        public static int GetNumberOfLineBreaks(this string text)
        {
            int lineBreaks = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    lineBreaks++;
                }
                else if (text[i] == '\r')
                {
                    if (i + 1 == text.Length || text[i + 1] != '\n')
                    {
                        lineBreaks++;
                    }
                }
            }

            return lineBreaks;
        }

        public static bool ContainsTab(this string text)
        {
            // PERF: Tried replacing this with "text.IndexOf('\t')>=0", but that was actually slightly slower
            foreach (char ch in text)
            {
                if (ch == '\t')
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
            return ConvertTabToSpace(line, tabSize, 0, endPosition);
        }

        public static int GetLineOffsetFromColumn(this string line, int column, int tabSize)
        {
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
    }
}