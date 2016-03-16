using System;
using System.Collections.Generic;
using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public sealed class IntrinsicScalarTypeSymbol : IntrinsicTypeSymbol
    {
        public IntrinsicScalarTypeSymbol(string name, string documentation, ScalarType scalarType, Func<TypeSymbol, IEnumerable<Symbol>> lazyMembers = null)
            : base(SymbolKind.IntrinsicScalarType, name, documentation, lazyMembers)
        {
            ScalarType = scalarType;
        }

        public ScalarType ScalarType { get; }
    }
}