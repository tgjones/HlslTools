namespace HlslTools.Symbols
{
    public sealed class IndexerSymbol : Symbol, IMemberSymbol
    {
        internal IndexerSymbol(string name, string documentation, TypeSymbol parent, TypeSymbol valueType)
            : base(SymbolKind.Indexer, name, documentation, parent)
        {
            ValueType = valueType;
        }

        public TypeSymbol ValueType { get; }

        TypeSymbol IMemberSymbol.AssociatedType => ValueType;
    }
}