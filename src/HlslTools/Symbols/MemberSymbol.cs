namespace HlslTools.Symbols
{
    public abstract class MemberSymbol : Symbol
    {
        public TypeSymbol AssociatedType { get; }

        protected MemberSymbol(SymbolKind kind, string name, string documentation, TypeSymbol parent, TypeSymbol associatedType)
            : base(kind, name, documentation, parent)
        {
            Parent = parent;
            AssociatedType = associatedType;
        }
    }
}