using System;
using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public class FunctionSymbol : InvocableSymbol
    {
        internal FunctionSymbol(string name, string documentation, Symbol parent, TypeSymbol returnType, Func<InvocableSymbol, IEnumerable<ParameterSymbol>> lazyParameters = null, bool isNumericConstructor = false)
            : base(SymbolKind.Function, name, documentation, parent, returnType, lazyParameters)
        {
            IsNumericConstructor = isNumericConstructor;
        }

        public bool IsNumericConstructor { get; }
    }
}