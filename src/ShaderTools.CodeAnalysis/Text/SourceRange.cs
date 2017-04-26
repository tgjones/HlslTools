using System;

namespace ShaderTools.CodeAnalysis.Text
{
    public struct SourceRange : IEquatable<SourceRange>
    {
        public SourceRange(SourceLocation start, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "");

            Start = start;
            Length = length;
        }

        public static SourceRange Union(SourceRange value1, SourceRange value2)
        {
            if (value1.Start.Position == 0 && value1.Length == 0)
                return value2;

            var startValue = (value1.Start < value2.Start) ? value1.Start : value2.Start;
            var endValue = (value1.End > value2.End) ? value1.End : value2.End;
            return FromBounds(startValue, endValue);
        }

        public static SourceRange FromBounds(SourceLocation start, SourceLocation end)
        {
            if (end < start)
                throw new ArgumentOutOfRangeException();
            var length = end - start;
            return new SourceRange(start, length);
        }

        public SourceLocation Start { get; }

        public SourceLocation End => Start + Length;

        public int Length { get; }

        public bool Contains(SourceLocation position)
        {
            return Start <= position && position < End;
        }

        public bool ContainsOrTouches(SourceLocation position)
        {
            return Contains(position) || position == End;
        }

        public bool Contains(SourceRange textSpan)
        {
            return Start <= textSpan.Start && textSpan.End <= End;
        }

        public bool Equals(SourceRange other)
        {
            return Start == other.Start &&
                   Length == other.Length;
        }

        public override bool Equals(object obj)
        {
            var other = obj as SourceRange?;
            return other != null && Equals(other.Value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Start.GetHashCode() * 397) ^ Length;
            }
        }

        public static bool operator ==(SourceRange left, SourceRange right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SourceRange left, SourceRange right)
        {
            return !left.Equals(right);
        }

        public override string ToString() => $"[{Start},{End})";
    }
}