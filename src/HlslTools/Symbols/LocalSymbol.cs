namespace HlslTools.Symbols
{
    public abstract class LocalSymbol : Symbol
    {
        public TypeSymbol ValueType { get; }

        protected LocalSymbol(SymbolKind kind, string name, string documentation, Symbol parent, TypeSymbol valueType)
            : base(kind, name, documentation, parent)
        {
            ValueType = valueType;
        }
    }
}