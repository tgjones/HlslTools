using System;
using System.Collections.Generic;
using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public sealed class IntrinsicVectorTypeSymbol : IntrinsicTypeSymbol
    {
        public IntrinsicVectorTypeSymbol(string name, string documentation, ScalarType scalarType, int numComponents, Func<TypeSymbol, IEnumerable<Symbol>> lazyMembers = null)
            : base(SymbolKind.IntrinsicVectorType, name, documentation, lazyMembers)
        {
            ScalarType = scalarType;
            NumComponents = numComponents;
        }

        public ScalarType ScalarType { get; }
        public int NumComponents { get; }
    }
}