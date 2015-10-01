using System;
using System.Collections.Generic;

namespace HlslTools.Symbols
{
    public class FunctionDeclarationSymbol : FunctionSymbol
    {
        public FunctionDeclarationSymbol(string name, string documentation, TypeSymbol returnType, Func<FunctionSymbol, IEnumerable<ParameterSymbol>> lazyParameters)
            : base(SymbolKind.FunctionDeclaration, name, documentation, returnType, lazyParameters)
        {
            
        }
    }
}