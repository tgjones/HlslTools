using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace HlslTools.Symbols
{
    public class MethodDefinitionSymbol : MethodSymbol
    {
        internal MethodDefinitionSymbol(string name, string documentation, TypeSymbol parent, TypeSymbol returnType, Func<MethodSymbol, IEnumerable<ParameterSymbol>> lazyParameters)
            : base(SymbolKind.MethodDefinition, name, documentation, parent, returnType, lazyParameters)
        {
            
        }
    }
}