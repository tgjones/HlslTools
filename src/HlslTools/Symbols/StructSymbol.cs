using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public sealed class StructSymbol : TypeSymbol
    {
        public StructTypeSyntax Syntax { get; }

        internal StructSymbol(StructTypeSyntax syntax, Symbol parent)
            : base(SymbolKind.Struct, (syntax.Name != null) ? syntax.Name.Text : "<anonymous struct>", string.Empty, parent)
        {
            Syntax = syntax;
        }
    }
}