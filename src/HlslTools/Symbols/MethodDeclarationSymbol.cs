using System;
using System.Collections.Generic;

namespace HlslTools.Symbols
{
    public class MethodDeclarationSymbol : MethodSymbol
    {
        public MethodDeclarationSymbol(string name, string documentation, TypeSymbol parent, TypeSymbol returnType, Func<MethodSymbol, IEnumerable<ParameterSymbol>> lazyParameters)
            : base(SymbolKind.MethodDeclaration, name, documentation, parent, returnType, lazyParameters)
        {
            
        }
    }
}