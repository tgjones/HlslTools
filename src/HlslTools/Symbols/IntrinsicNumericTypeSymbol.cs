using System;
using System.Collections.Generic;
using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public abstract class IntrinsicNumericTypeSymbol : IntrinsicTypeSymbol
    {
        public ScalarType ScalarType { get; }

        protected IntrinsicNumericTypeSymbol(SymbolKind kind, string name, string documentation, ScalarType scalarType, Func<TypeSymbol, IEnumerable<Symbol>> lazyMembers = null)
            : base(kind, name, documentation, lazyMembers)
        {
            ScalarType = scalarType;
        }
    }
}