using System;
using System.Collections.Generic;

namespace HlslTools.Symbols
{
    public abstract class IntrinsicTypeSymbol : TypeSymbol
    {
        private readonly Func<TypeSymbol, IEnumerable<Symbol>> _lazyMembers;

        protected IntrinsicTypeSymbol(SymbolKind kind, string name, string documentation, Func<TypeSymbol, IEnumerable<Symbol>> lazyMembers = null)
            : base(kind, name, documentation, null)
        {
            _lazyMembers = lazyMembers;
        }

        protected override void ComputeLazyMembers()
        {
            if (_lazyMembers != null)
                foreach (var member in _lazyMembers(this))
                    AddMember(member);
        }
    }
}