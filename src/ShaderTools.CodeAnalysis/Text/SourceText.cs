using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace ShaderTools.CodeAnalysis.Text
{
    public abstract class SourceText
    {
        public static SourceText From(string text, string filename = null)
        {
            return new StringText(text, filename);
        }

        public abstract string GetText(TextSpan textSpan);

        public string GetText(int position, int length)
        {
            return GetText(new TextSpan(this, position, length));
        }

        public string GetText()
        {
            return GetText(0, Length);
        }

        public abstract char this[int index] { get; }

        public abstract int Length { get; }

        public abstract TextLineCollection Lines { get; }

        public abstract string Filename { get; }

        public TextLine GetLineFromPosition(int position)
        {
            if (position < 0 || position > Length)
                throw new ArgumentOutOfRangeException(nameof(position));

            var lineNumber = GetLineNumberFromPosition(position);
            return Lines[lineNumber];
        }

        public abstract int GetLineNumberFromPosition(int position);

        public TextLocation GetTextLocation(int position)
        {
            var line = GetLineFromPosition(position);
            var lineNumber = line.LineNumber;
            var column = position - line.Span.Start;
            return new TextLocation(lineNumber, column);
        }

        public int GetPosition(TextLocation location)
        {
            var textLine = Lines[location.Line];
            return textLine.Span.Start + location.Column;
        }

        public abstract bool IsRoot { get; }

        public SourceText WithChanges(params TextChange[] changes)
        {
            if (changes == null || changes.Length == 0)
                return this;

            return WithChanges((IEnumerable<TextChange>)changes);
        }

        public SourceText WithChanges(IEnumerable<TextChange> changes)
        {
            if (changes == null)
                throw new ArgumentNullException(nameof(changes));

            var persistedChanges = changes.OrderByDescending(c => c.Span.Start)
                                          .ToImmutableArray();


            var sb = new StringBuilder(GetText());
            var hasChanges = false;
            var previousStart = int.MaxValue;

            foreach (var textChange in persistedChanges)
            {
                if (textChange.Span.End > previousStart)
                    throw new InvalidOperationException("Source text changes must not overlap.");

                previousStart = textChange.Span.Start;

                if (!HasDifference(sb, textChange))
                    continue;

                hasChanges = true;
                sb.Remove(textChange.Span.Start, textChange.Span.Length);
                sb.Insert(textChange.Span.Start, textChange.NewText);
            }

            if (!hasChanges)
                return this;

            var newText = From(sb.ToString(), Filename);

            return newText;
            //return new ChangedSourceText(this, newText, persistedChanges);
        }

        private static bool HasDifference(StringBuilder sb, TextChange textChange)
        {
            var newText = textChange.NewText ?? string.Empty;
            var newLength = newText.Length;
            var oldLength = textChange.Span.Length;

            if (oldLength != newLength)
                return true;

            var bufferStart = textChange.Span.Start;
            var bufferEnd = textChange.Span.End;

            for (var bufferIndex = bufferStart; bufferIndex < bufferEnd; bufferIndex++)
            {
                var newTextIndex = bufferIndex - bufferStart;
                if (sb[bufferIndex] != newText[newTextIndex])
                    return true;
            }

            return false;
        }

        public abstract SourceText WithFilename(string newFilename);
    }
}