using System;
using System.Collections.Generic;

namespace HlslTools.Symbols
{
    public class FunctionDefinitionSymbol : FunctionSymbol
    {
        public FunctionDefinitionSymbol(string name, string documentation, TypeSymbol returnType, Func<FunctionSymbol, IEnumerable<ParameterSymbol>> lazyParameters)
            : base(SymbolKind.FunctionDefinition, name, documentation, returnType, lazyParameters)
        {

        }
    }
}