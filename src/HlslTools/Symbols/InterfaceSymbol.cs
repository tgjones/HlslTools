using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public sealed class InterfaceSymbol : TypeSymbol
    {
        internal InterfaceSymbol(InterfaceTypeSyntax syntax, Symbol parent)
            : base(SymbolKind.Interface, syntax.Name.Text, string.Empty, parent)
        {
        }
    }
}