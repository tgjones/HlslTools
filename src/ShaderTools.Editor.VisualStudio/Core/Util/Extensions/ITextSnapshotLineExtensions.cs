using System;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;

namespace ShaderTools.Editor.VisualStudio.Core.Util.Extensions
{
    // From https://github.com/dotnet/roslyn/blob/master/src/EditorFeatures/Text/Shared/Extensions/ITextSnapshotLineExtensions.cs
    internal static class ITextSnapshotLineExtensions
    {
        /// <summary>
        /// Returns the first non-whitespace position on the given line, or null if 
        /// the line is empty or contains only whitespace.
        /// </summary>
        public static int? GetFirstNonWhitespacePosition(this ITextSnapshotLine line)
        {
            var text = line.GetText();

            for (int i = 0; i < text.Length; i++)
            {
                if (!char.IsWhiteSpace(text[i]))
                {
                    return line.Start + i;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the first non-whitespace position on the given line as an offset
        /// from the start of the line, or null if the line is empty or contains only
        /// whitespace.
        /// </summary>
        public static int? GetFirstNonWhitespaceOffset(this ITextSnapshotLine line)
        {
            var text = line.GetText();

            for (int i = 0; i < text.Length; i++)
            {
                if (!char.IsWhiteSpace(text[i]))
                {
                    return i;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the last non-whitespace position on the given line, or null if 
        /// the line is empty or contains only whitespace.
        /// </summary>
        public static int? GetLastNonWhitespacePosition(this ITextSnapshotLine line)
        {
            int startPosition = line.Start;
            var text = line.GetText();

            for (int i = text.Length - 1; i >= 0; i--)
            {
                if (!char.IsWhiteSpace(text[i]))
                {
                    return startPosition + i;
                }
            }

            return null;
        }

        /// <summary>
        /// Determines whether the specified line is empty or contains whitespace only.
        /// </summary>
        public static bool IsEmptyOrWhitespace(this ITextSnapshotLine line)
        {
            var text = line.GetText();

            for (int i = 0; i < text.Length; i++)
            {
                if (!char.IsWhiteSpace(text[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static ITextSnapshotLine GetPreviousMatchingLine(this ITextSnapshotLine line, Func<ITextSnapshotLine, bool> predicate)
        {
            if (line.LineNumber <= 0)
            {
                return null;
            }

            var snapshot = line.Snapshot;
            for (int lineNumber = line.LineNumber - 1; lineNumber >= 0; lineNumber--)
            {
                var currentLine = snapshot.GetLineFromLineNumber(lineNumber);
                if (!predicate(currentLine))
                {
                    continue;
                }

                return currentLine;
            }

            return null;
        }

        public static int GetColumnOfFirstNonWhitespaceCharacterOrEndOfLine(this ITextSnapshotLine line, IEditorOptions editorOptions)
        {
            return line.GetColumnOfFirstNonWhitespaceCharacterOrEndOfLine(editorOptions.GetTabSize());
        }

        public static int GetColumnOfFirstNonWhitespaceCharacterOrEndOfLine(this ITextSnapshotLine line, int tabSize)
        {
            return line.GetText().GetColumnOfFirstNonWhitespaceCharacterOrEndOfLine(tabSize);
        }

        public static int GetColumnFromLineOffset(this ITextSnapshotLine line, int lineOffset, IEditorOptions editorOptions)
        {
            return line.GetText().GetColumnFromLineOffset(lineOffset, editorOptions.GetTabSize());
        }

        public static int GetLineOffsetFromColumn(this ITextSnapshotLine line, int column, IEditorOptions editorOptions)
        {
            return line.GetText().GetLineOffsetFromColumn(column, editorOptions.GetTabSize());
        }
    }
}