using System;

namespace ShaderTools.Core.Symbols.Markup
{
    public sealed class SymbolMarkupToken : IEquatable<SymbolMarkupToken>
    {
        public SymbolMarkupToken(SymbolMarkupKind kind, string text)
        {
            Kind = kind;
            Text = text;
        }

        public SymbolMarkupKind Kind { get; }

        public string Text { get; }

        public bool Equals(SymbolMarkupToken other)
        {
            return Kind == other.Kind && string.Equals(Text, other.Text);
        }

        public override bool Equals(object obj)
        {
            var other = obj as SymbolMarkupToken;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Kind * 397) ^ Text.GetHashCode();
            }
        }
    }
}