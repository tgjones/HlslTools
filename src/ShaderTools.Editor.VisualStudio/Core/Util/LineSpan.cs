using System;

namespace ShaderTools.Editor.VisualStudio.Core.Util
{
    // From https://github.com/dotnet/roslyn/blob/master/src/Workspaces/Core/Portable/Shared/Extensions/LineSpan.cs
    // Like Span, except it has a start/end line instead of a start/end position.
    internal struct LineSpan : IEquatable<LineSpan>
    {
        // inclusive
        public int Start { get; private set; }

        // exclusive
        public int End { get; private set; }

        public static LineSpan FromBounds(int start, int end)
        {
            var result = new LineSpan();
            result.Start = start;
            result.End = end;
            return result;
        }

        public bool Equals(LineSpan other)
        {
            return this.Start == other.Start && this.End == other.End;
        }

        public override bool Equals(object obj)
        {
            return this.Equals((LineSpan)obj);
        }

        public override int GetHashCode()
        {
            return Hash.Combine(this.Start, this.End);
        }
    }
}