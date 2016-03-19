using System;
using System.Collections.Generic;
using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public sealed class IntrinsicScalarTypeSymbol : IntrinsicNumericTypeSymbol
    {
        public IntrinsicScalarTypeSymbol(string name, string documentation, ScalarType scalarType, Func<TypeSymbol, IEnumerable<Symbol>> lazyMembers = null)
            : base(SymbolKind.IntrinsicScalarType, name, documentation, scalarType, lazyMembers)
        {
            
        }
    }
}