using System;

namespace HlslTools.Text
{
    public struct TextSpan : IEquatable<TextSpan>
    {
        public static readonly TextSpan None = new TextSpan(SourceText.From(string.Empty), -1, 0);

        public TextSpan(SourceText sourceText, int start, int length)
        {
            SourceText = sourceText;
            IsInRootFile = sourceText?.Filename == null;
            Filename = sourceText?.Filename;
            Start = start;
            End = Start + length;
            Length = length;
        }

        public readonly SourceText SourceText;
        public readonly bool IsInRootFile;
        public readonly string Filename;
        public readonly int Start;
        public readonly int End;
        public readonly int Length;

        public static TextSpan Union(TextSpan value1, TextSpan value2)
        {
            if (value1.Start == 0 && value1.Length == 0)
                return value2;

            var startValue = (value1.Start < value2.Start) ? value1.Start : value2.Start;
            var endValue = (value1.End > value2.End) ? value1.End : value2.End;
            return FromBounds(value1.SourceText, startValue, endValue);
        }

        public static TextSpan FromBounds(SourceText sourceText, int start, int end)
        {
            if (end < start)
                throw new ArgumentOutOfRangeException();
            var length = end - start;
            return new TextSpan(sourceText, start, length);
        }

        public bool Contains(int position)
        {
            return Start <= position && position < End;
        }

        public bool ContainsOrTouches(int position)
        {
            return Contains(position) || position == End;
        }

        public bool Contains(TextSpan textSpan)
        {
            return Start <= textSpan.Start && textSpan.End <= End;
        }

        public bool OverlapsWith(TextSpan span)
        {
            var maxStart = Math.Max(Start, span.Start);
            var minEnd = Math.Min(End, span.End);
            return maxStart < minEnd;
        }

        public bool IntersectsWith(TextSpan span)
        {
            return span.Start <= End && span.End >= Start;
        }

        public bool Equals(TextSpan other)
        {
            return Start == other.Start &&
                   Length == other.Length;
        }

        public override bool Equals(object obj)
        {
            var other = obj as TextSpan?;
            return other != null && Equals(other.Value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Start * 397) ^ Length;
            }
        }

        public static bool operator ==(TextSpan left, TextSpan right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextSpan left, TextSpan right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            var result = Filename;
            if (!string.IsNullOrEmpty(result))
                result += " ";
            result += $"[{Start},{End})";
            return result;
        }
    }
}