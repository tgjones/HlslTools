namespace HlslTools.Symbols
{
    public sealed class AttributeSymbol : InvocableSymbol
    {
        public AttributeSymbol(string name, string documentation)
            : base(SymbolKind.Attribute, name, documentation, null, TypeFacts.Missing)
        {
        }
    }
}