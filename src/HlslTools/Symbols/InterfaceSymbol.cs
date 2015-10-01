using System;
using System.Collections.Generic;
using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public sealed class InterfaceSymbol : TypeSymbol
    {
        internal InterfaceSymbol(InterfaceTypeSyntax syntax, Symbol parent, Func<TypeSymbol, IEnumerable<MethodDeclarationSymbol>> lazyMethods)
            : base(SymbolKind.Interface, syntax.Name.Text, string.Empty, parent, lazyMethods)
        {
        }
    }
}