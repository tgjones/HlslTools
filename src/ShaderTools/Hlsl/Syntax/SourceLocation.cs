namespace ShaderTools.Hlsl.Syntax
{
    public struct SourceLocation
    {
        internal SourceLocation(int position)
        {
            Position = position;
        }

        internal int Position { get; }

        public override bool Equals(object obj)
        {
            var other = obj as SourceLocation?;
            return other != null && Equals(other.Value);
        }

        public bool Equals(SourceLocation other)
        {
            return Position == other.Position;
        }

        public static bool operator ==(SourceLocation left, SourceLocation right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SourceLocation left, SourceLocation right)
        {
            return !left.Equals(right);
        }

        public static bool operator <=(SourceLocation left, SourceLocation right)
        {
            return left.Position <= right.Position;
        }

        public static bool operator<(SourceLocation left, SourceLocation right)
        {
            return left.Position < right.Position;
        }

        public static bool operator >=(SourceLocation left, SourceLocation right)
        {
            return left.Position >= right.Position;
        }

        public static bool operator>(SourceLocation left, SourceLocation right)
        {
            return left.Position > right.Position;
        }

        public static SourceLocation operator+(SourceLocation left, int length)
        {
            return new SourceLocation(left.Position + length);
        }

        public static int operator-(SourceLocation left, SourceLocation right)
        {
            return left.Position - right.Position;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }

        public override string ToString()
        {
            return Position.ToString();
        }

        public static explicit operator int(SourceLocation c)
        {
            return c.Position;
        }
    }
}