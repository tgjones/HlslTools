using System;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.Utilities;

namespace ShaderTools.CodeAnalysis.Text
{
    public struct SourceFileSpan
    {
        public SourceFile File { get; }
        public TextSpan Span { get; }

        public bool IsInRootFile => File.IsRootFile;

        public SourceFileSpan(SourceFile file, TextSpan span)
        {
            File = file ?? throw new ArgumentNullException(nameof(file));
            Span = span;
        }

        /// <summary>
        /// Determines if two instances of <see cref="SourceFileSpan"/> are the same.
        /// </summary>
        public static bool operator ==(SourceFileSpan left, SourceFileSpan right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines if two instances of <see cref="SourceFileSpan"/> are different.
        /// </summary>
        public static bool operator !=(SourceFileSpan left, SourceFileSpan right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Determines if current instance of <see cref="SourceFileSpan"/> is equal to another.
        /// </summary>
        public bool Equals(SourceFileSpan other)
        {
            return File == other.File && Span == other.Span;
        }

        /// <summary>
        /// Determines if current instance of <see cref="SourceFileSpan"/> is equal to another.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is SourceFileSpan && Equals((SourceFileSpan)obj);
        }

        /// <summary>
        /// Produces a hash code for <see cref="SourceFileSpan"/>.
        /// </summary>
        public override int GetHashCode()
        {
            return Hash.Combine(File.GetHashCode(), Span.GetHashCode());
        }
    }
}
