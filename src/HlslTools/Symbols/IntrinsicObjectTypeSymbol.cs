using System;
using System.Collections.Generic;
using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public sealed class IntrinsicObjectTypeSymbol : IntrinsicTypeSymbol
    {
        public PredefinedObjectType PredefinedType { get; }

        public IntrinsicObjectTypeSymbol(string name, string documentation, PredefinedObjectType predefinedType, Func<TypeSymbol, IEnumerable<Symbol>> lazyMembers = null)
            : base(SymbolKind.IntrinsicObjectType, name, documentation, lazyMembers)
        {
            PredefinedType = predefinedType;
        }
    }
}