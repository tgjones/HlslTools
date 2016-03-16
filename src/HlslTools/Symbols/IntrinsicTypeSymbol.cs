using System;
using System.Collections.Generic;

namespace HlslTools.Symbols
{
    public abstract class IntrinsicTypeSymbol : TypeSymbol
    {
        protected IntrinsicTypeSymbol(SymbolKind kind, string name, string documentation, Func<TypeSymbol, IEnumerable<Symbol>> lazyMembers = null)
            : base(kind, name, documentation, null)
        {
            if (lazyMembers != null)
                foreach (var member in lazyMembers(this))
                    AddMember(member);
        }
    }
}