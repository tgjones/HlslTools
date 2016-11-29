using System;
using ShaderTools.Hlsl.Text;

namespace ShaderTools.VisualStudio.Hlsl.IntelliSense.SignatureHelp
{
    public sealed class ParameterItem : IEquatable<ParameterItem>
    {
        public ParameterItem(string name, string documentation, TextSpan span)
        {
            Name = name;
            Documentation = documentation;
            Span = span;
        }

        public string Name { get; }
        public string Documentation { get; }
        public TextSpan Span { get; }

        public bool Equals(ParameterItem other)
        {
            return other != null &&
                   Name == other.Name &&
                   Span == other.Span;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ParameterItem;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Span.GetHashCode();
                return hashCode;
            }
        }
    }
}