using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public sealed class ClassSymbol : TypeSymbol
    {
        public ClassSymbol(ClassTypeSyntax syntax, Symbol parent)
            : base(SymbolKind.Class, syntax.Name.Text, string.Empty, parent)
        {
        }
    }
}