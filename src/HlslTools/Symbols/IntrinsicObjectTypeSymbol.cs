using System;
using System.Collections.Generic;

namespace HlslTools.Symbols
{
    public sealed class IntrinsicObjectTypeSymbol : IntrinsicTypeSymbol
    {
        public IntrinsicObjectTypeSymbol(string name, string documentation, Func<TypeSymbol, IEnumerable<Symbol>> lazyMembers = null)
            : base(SymbolKind.IntrinsicObjectType, name, documentation, lazyMembers)
        {
            
        }
    }
}