namespace HlslTools.Symbols
{
    public sealed class IndexerSymbol : InvocableSymbol
    {
        internal IndexerSymbol(string name, string documentation, TypeSymbol parent, TypeSymbol valueType)
            : base(SymbolKind.Indexer, name, documentation, parent, valueType)
        {
            ValueType = valueType;
        }

        public TypeSymbol ValueType { get; }
    }
}