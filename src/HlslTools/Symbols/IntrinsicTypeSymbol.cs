namespace HlslTools.Symbols
{
    public abstract class IntrinsicTypeSymbol : TypeSymbol
    {
        protected IntrinsicTypeSymbol(SymbolKind kind, string name, string documentation)
            : base(kind, name, documentation, null)
        {
            
        }
    }
}