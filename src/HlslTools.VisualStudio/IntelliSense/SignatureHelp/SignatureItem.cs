using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace HlslTools.VisualStudio.IntelliSense.SignatureHelp
{
    internal sealed class SignatureItem : IEquatable<SignatureItem>
    {
        private readonly string _content;
        private readonly ImmutableArray<ParameterItem> _parameters;

        public SignatureItem(string content, IEnumerable<ParameterItem> parameters)
        {
            _content = content;
            _parameters = parameters.ToImmutableArray();
        }

        public string Content
        {
            get { return _content; }
        }

        public ImmutableArray<ParameterItem> Parameters
        {
            get { return _parameters; }
        }

        public bool Equals(SignatureItem other)
        {
            return other != null &&
                   _content == other.Content &&
                   _parameters.SequenceEqual(other.Parameters);
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
                var hashCode = _content.GetHashCode();
                hashCode = (hashCode * 397) ^ _parameters.GetHashCode();
                return hashCode;
            }
        }
    }
}