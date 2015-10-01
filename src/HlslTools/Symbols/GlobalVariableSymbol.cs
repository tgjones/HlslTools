using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public class GlobalVariableSymbol : GlobalSymbol
    {
        internal GlobalVariableSymbol(VariableDeclaratorSyntax syntax, TypeSymbol valueType)
            : base(SymbolKind.GlobalVariable, syntax.Identifier.Text, string.Empty, valueType)
        {
            
        }
    }
}