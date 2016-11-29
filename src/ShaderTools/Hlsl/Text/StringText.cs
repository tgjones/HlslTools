using System;
using System.Collections.Generic;

namespace ShaderTools.Hlsl.Text
{
    internal sealed class StringText : SourceText
    {
        private readonly string _text;
        private readonly StringTextLineCollection _lines;

        public StringText(string text, string filename = null)
        {
            _text = text;
            Filename = filename;
            _lines = Parse(this, text);
        }

        private static StringTextLineCollection Parse(SourceText sourceText, string text)
        {
            var textLines = new List<TextLine>();
            var position = 0;
            var lineStart = 0;

            while (position < text.Length)
            {
                var lineBreakWidth = GetLineBreakWidth(text, position);

                if (lineBreakWidth == 0)
                {
                    position++;
                }
                else
                {
                    AddLine(sourceText, textLines, lineStart, position);

                    position += lineBreakWidth;
                    lineStart = position;
                }
            }

            if (lineStart <= position)
                AddLine(sourceText, textLines, lineStart, text.Length);

            return new StringTextLineCollection(textLines);
        }

        private static void AddLine(SourceText sourceText, List<TextLine> textLines, int lineStart, int lineEnd)
        {
            var lineLength = lineEnd - lineStart;
            var textLine = new TextLine(sourceText, lineStart, lineLength);
            textLines.Add(textLine);
        }

        private static int GetLineBreakWidth(string text, int position)
        {
            const char eof = '\0';
            const char cr = '\r';
            const char lf = '\n';

            var n = position + 1;
            var c = text[position];
            var l = n < text.Length ? text[n] : eof;

            if (c == cr && l == lf)
                return 2;

            if (c == cr || c == lf)
                return 1;

            return 0;
        }

        public override string GetText(TextSpan textSpan)
        {
            return _text.Substring(textSpan.Start, textSpan.Length);
        }

        public override int Length => _text.Length;
        public override char this[int index] => _text[index];
        public override string Filename { get; }

        public override int GetLineNumberFromPosition(int position)
        {
            if (position < 0 || position > Length)
                throw new ArgumentOutOfRangeException(nameof(position));

            var lower = 0;
            var upper = _lines.Count - 1;
            while (lower <= upper)
            {
                var index = lower + ((upper - lower) >> 1);
                var current = _lines[index];
                var start = current.Span.Start;
                if (start == position)
                    return index;

                if (start > position)
                    upper = index - 1;
                else
                    lower = index + 1;
            }

            return lower - 1;
        }
    }

    public struct TextLine : IEquatable<TextLine>
    {
        private readonly SourceText _text;
        private readonly int _start;
        private readonly int _length;

        public TextLine(SourceText text, int start, int length)
        {
            _text = text;
            _start = start;
            _length = length;
        }

        public SourceText Text
        {
            get { return _text; }
        }

        public TextSpan Span => new TextSpan(_text, _start, _length);

        public int LineNumber => _text.GetLineNumberFromPosition(_start);

        public string GetText()
        {
            return _text.GetText(_start, _length);
        }

        public bool Equals(TextLine other)
        {
            return _text == other._text &&
                   _start == other._start &&
                   _length == other._length;
        }

        public override bool Equals(object obj)
        {
            var other = obj as TextLine?;
            return other != null && Equals(other.Value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_text != null ? _text.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _start;
                hashCode = (hashCode * 397) ^ _length;
                return hashCode;
            }
        }

        public static bool operator ==(TextLine left, TextLine right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextLine left, TextLine right)
        {
            return !left.Equals(right);
        }
    }
}