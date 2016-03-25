namespace HlslTools.Symbols
{
    public sealed class IndexerSymbol : InvocableSymbol
    {
        internal IndexerSymbol(string name, string documentation, TypeSymbol parent, TypeSymbol indexType, TypeSymbol valueType, bool readOnly = true)
            : base(SymbolKind.Indexer, name, documentation, parent, valueType)
        {
            IndexType = indexType;
            ValueType = valueType;
            ReadOnly = readOnly;
        }

        public TypeSymbol IndexType { get; }
        public TypeSymbol ValueType { get; }
        public bool ReadOnly { get; }
    }
}