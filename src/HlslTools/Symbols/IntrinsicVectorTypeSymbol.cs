using System;
using System.Collections.Generic;
using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public sealed class IntrinsicVectorTypeSymbol : IntrinsicNumericTypeSymbol
    {
        public IntrinsicVectorTypeSymbol(string name, string documentation, ScalarType scalarType, int numComponents, Func<TypeSymbol, IEnumerable<Symbol>> lazyMembers = null)
            : base(SymbolKind.IntrinsicVectorType, name, documentation, scalarType, lazyMembers)
        {
            NumComponents = numComponents;
        }

        public int NumComponents { get; }
    }
}