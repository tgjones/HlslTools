namespace HlslTools.Symbols
{
    public abstract class GlobalSymbol : Symbol
    {
        public TypeSymbol ValueType { get; }

        protected GlobalSymbol(SymbolKind kind, string name, string documentation, TypeSymbol valueType)
            : base(kind, name, documentation, null)
        {
            ValueType = valueType;
        }
    }
}