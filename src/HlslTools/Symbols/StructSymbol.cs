using System;
using System.Collections.Generic;
using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public sealed class StructSymbol : TypeSymbol
    {
        internal StructSymbol(StructTypeSyntax syntax, Symbol parent, Func<TypeSymbol, IEnumerable<FieldSymbol>> lazyFields)
            : base(SymbolKind.Struct, syntax.Name.Text, string.Empty, parent, lazyFields)
        {
        }
    }
}