using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace HlslTools.Syntax
{
    public struct SeparatedSyntaxList<TNode> : IEquatable<SeparatedSyntaxList<TNode>>, IReadOnlyList<TNode>
        where TNode : SyntaxNode
    {
        private readonly List<SyntaxNode> _list;

        internal SeparatedSyntaxList(List<SyntaxNode> list)
        {
            Validate(list);
            _list = list;
        }

        [Conditional("DEBUG")]
        private static void Validate(List<SyntaxNode> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if ((i & 1) != 0)
                    Debug.Assert(item is SyntaxToken, "odd elements of a separated list must be tokens");
            }
        }

        public int Count => (_list.Count + 1) >> 1;

        public int SeparatorCount => _list.Count >> 1;

        public TNode this[int index] => (TNode)_list[index << 1];

        /// <summary>
        /// Gets the separator at the given index in this list.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        [Pure]
        public SyntaxToken GetSeparator(int index)
        {
            return (SyntaxToken) _list[(index << 1) + 1];
        }

        [Pure]
        public List<SyntaxNode> GetWithSeparators()
        {
            return _list;
        }

        [Pure]
        public IEnumerable<SyntaxToken> GetSeparators()
        {
            return _list.Where((item, index) => index % 2 != 0).Cast<SyntaxToken>();
        } 

        public static bool operator ==(SeparatedSyntaxList<TNode> left, SeparatedSyntaxList<TNode> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SeparatedSyntaxList<TNode> left, SeparatedSyntaxList<TNode> right)
        {
            return !left.Equals(right);
        }

        public bool Equals(SeparatedSyntaxList<TNode> other)
        {
            return _list == other._list;
        }

        public IEnumerator<TNode> GetEnumerator()
        {
            return new EnumeratorImpl(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            return (obj is SeparatedSyntaxList<TNode>) && Equals((SeparatedSyntaxList<TNode>)obj);
        }

        public override int GetHashCode()
        {
            return _list.GetHashCode();
        }

        private sealed class EnumeratorImpl : IEnumerator<TNode>
        {
            private readonly SeparatedSyntaxList<TNode> _list;
            private int _index;

            internal EnumeratorImpl(SeparatedSyntaxList<TNode> list)
            {
                _list = list;
                _index = -1;
            }

            public TNode Current => _list[_index];

            object IEnumerator.Current => _list[_index];

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                int newIndex = _index + 1;
                if (newIndex < _list.Count)
                {
                    _index = newIndex;
                    return true;
                }

                return false;
            }

            public void Reset()
            {
                _index = -1;
            }
        }
    }
}