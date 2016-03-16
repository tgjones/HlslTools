using System;
using System.Collections.Generic;

namespace HlslTools.Symbols
{
    public class MethodSymbol : InvocableSymbol, IMemberSymbol
    {
        public MethodSymbol(string name, string documentation, TypeSymbol parent, TypeSymbol returnType, Func<InvocableSymbol, IEnumerable<ParameterSymbol>> lazyParameters = null)
            : base(SymbolKind.Method, name, documentation, parent, returnType, lazyParameters)
        {
        }

        TypeSymbol IMemberSymbol.AssociatedType => ReturnType;
    }
}