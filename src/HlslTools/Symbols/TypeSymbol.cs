namespace HlslTools.Symbols
{
    public abstract class TypeSymbol : ContainerSymbol
    {
        internal TypeSymbol(SymbolKind kind, string name, string documentation, Symbol parent)
            : base(kind, name, documentation, parent)
        {
            
        }

        public virtual TypeSymbol GetBaseType()
        {
            return null;
        }
    }
}