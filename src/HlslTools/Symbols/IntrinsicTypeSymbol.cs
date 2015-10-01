using System;
using System.Collections.Generic;
using System.Linq;

namespace HlslTools.Symbols
{
    public class IntrinsicTypeSymbol : TypeSymbol
    {
        public IntrinsicTypeSymbol(string name, string documentation, Func<TypeSymbol, IEnumerable<MemberSymbol>> lazyMembers = null)
            : base(SymbolKind.IntrinsicType, name, documentation, null, lazyMembers ?? (t => Enumerable.Empty<MemberSymbol>()))
        {
            
        }
    }
}