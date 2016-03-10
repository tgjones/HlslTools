using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public class VariableSymbol : Symbol
    {
        internal VariableSymbol(VariableDeclaratorSyntax syntax, TypeSymbol parent, TypeSymbol valueType)
            : base(SymbolKind.Variable, syntax.Identifier.Text, string.Empty, parent)
        {
            ValueType = valueType;
        }

        protected VariableSymbol(SymbolKind kind, string name, string documentation, Symbol parent, TypeSymbol valueType)
            : base(kind, name, documentation, parent)
        {
            ValueType = valueType;
        }

        public TypeSymbol ValueType { get; }
    }
}