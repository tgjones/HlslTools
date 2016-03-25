namespace HlslTools.Symbols
{
    public sealed class IndexerSymbol : InvocableSymbol
    {
        internal IndexerSymbol(string name, string documentation, TypeSymbol parent, TypeSymbol valueType, bool readOnly = true)
            : base(SymbolKind.Indexer, name, documentation, parent, valueType)
        {
            ValueType = valueType;
            ReadOnly = readOnly;
        }

        public TypeSymbol ValueType { get; }
        public bool ReadOnly { get; }
    }
}