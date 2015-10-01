namespace HlslTools.Symbols
{
    public class IndexerSymbol : MemberSymbol
    {
        public IndexerSymbol(string name, string documentation, TypeSymbol parent, TypeSymbol valueType)
            : base(SymbolKind.Indexer, name, documentation, parent, valueType)
        {
            
        }
    }
}