using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public sealed class StructSymbol : TypeSymbol
    {
        internal StructSymbol(StructTypeSyntax syntax, Symbol parent)
            : base(SymbolKind.Struct, syntax.Name.Text, string.Empty, parent)
        {
        }
    }
}