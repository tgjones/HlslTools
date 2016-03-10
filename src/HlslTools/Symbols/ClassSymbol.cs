using System;
using System.Collections.Generic;
using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public sealed class ClassSymbol : TypeSymbol
    {
        public ClassSymbol(ClassTypeSyntax syntax, Symbol parent, Func<TypeSymbol, IEnumerable<Symbol>> lazyMembers)
            : base(SymbolKind.Class, syntax.Name.Text, string.Empty, parent, lazyMembers)
        {
        }
    }
}