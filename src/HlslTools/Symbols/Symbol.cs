using HlslTools.Symbols.Markup;

namespace HlslTools.Symbols
{
    public abstract class Symbol
    {
        public SymbolKind Kind { get; }
        public string Name { get; }
        public string Documentation { get; }
        public Symbol Parent { get; }

        internal Symbol(SymbolKind kind, string name, string documentation, Symbol parent)
        {
            Kind = kind;
            Name = name;
            Documentation = documentation;
            Parent = parent;
        }

        public sealed override string ToString()
        {
            return SymbolMarkup.ForSymbol(this).ToString();
        }

        protected bool EqualsImpl(Symbol other)
        {
            return Kind == other.Kind
                && string.Equals(Name, other.Name)
                && (Parent == null) == (other.Parent == null)
                && (Parent == null || Parent.EqualsImpl(other.Parent));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return EqualsImpl((Symbol) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Kind;
                hashCode = (hashCode * 397) ^ Name.GetHashCode();
                hashCode = (hashCode * 397) ^ (Parent?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}