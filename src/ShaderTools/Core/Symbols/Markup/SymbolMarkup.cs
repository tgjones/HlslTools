using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ShaderTools.Core.Symbols.Markup
{
    public sealed class SymbolMarkup : IEquatable<SymbolMarkup>
    {
        public SymbolMarkup(IEnumerable<SymbolMarkupToken> tokens)
        {
            Tokens = tokens.ToImmutableArray();
        }

        public ImmutableArray<SymbolMarkupToken> Tokens { get; }

        public override bool Equals(object obj)
        {
            var other = obj as SymbolMarkup;
            return other != null && Equals(other);
        }

        public bool Equals(SymbolMarkup other)
        {
            if (other.Tokens.Length != Tokens.Length)
                return false;

            for (var i = 0; i < Tokens.Length; i++)
            {
                if (!Tokens[i].Equals(other.Tokens[i]))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return Tokens.GetHashCode();
        }

        public override string ToString()
        {
            return string.Concat(Tokens.Select(n => n.Text));
        }
    }
}