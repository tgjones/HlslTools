using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using HlslTools.Symbols;

namespace HlslTools.VisualStudio.IntelliSense.SignatureHelp
{
    internal sealed class SignatureItem : IEquatable<SignatureItem>
    {
        public SignatureItem(Symbol symbol, string content, string documentation, IEnumerable<ParameterItem> parameters)
        {
            Symbol = symbol;
            Content = content;
            Documentation = documentation;
            Parameters = parameters.ToImmutableArray();
        }

        public Symbol Symbol { get; }
        public string Content { get; }
        public string Documentation { get; }
        public ImmutableArray<ParameterItem> Parameters { get; }

        public bool Equals(SignatureItem other)
        {
            return other != null &&
                   Content == other.Content &&
                   Parameters.SequenceEqual(other.Parameters);
        }

        public override bool Equals(object obj)
        {
            var other = obj as SignatureItem;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Content.GetHashCode();
                hashCode = (hashCode * 397) ^ Parameters.GetHashCode();
                return hashCode;
            }
        }
    }
}