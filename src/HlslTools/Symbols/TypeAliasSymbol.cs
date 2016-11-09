using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public sealed class TypeAliasSymbol : TypeSymbol
    {
        internal TypeAliasSymbol(TypeAliasSyntax syntax, TypeSymbol valueType)
            : base(SymbolKind.TypeAlias, syntax.Identifier.Text, string.Empty, null)
        {
            ValueType = valueType;
        }

        public TypeSymbol ValueType { get; }
    }
}