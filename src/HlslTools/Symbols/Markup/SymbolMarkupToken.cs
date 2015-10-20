using System;

namespace HlslTools.Symbols.Markup
{
    public sealed class SymbolMarkupToken : IEquatable<SymbolMarkupToken>
    {
        private readonly SymbolMarkupKind _kind;
        private readonly string _text;

        public SymbolMarkupToken(SymbolMarkupKind kind, string text)
        {
            _kind = kind;
            _text = text;
        }

        public SymbolMarkupKind Kind
        {
            get { return _kind; }
        }

        public string Text
        {
            get { return _text; }
        }

        public bool Equals(SymbolMarkupToken other)
        {
            return _kind == other._kind &&
                   string.Equals(_text, other._text);
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
                return ((int)_kind * 397) ^ _text.GetHashCode();
            }
        }
    }
}