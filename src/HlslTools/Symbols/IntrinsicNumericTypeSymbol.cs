namespace HlslTools.Symbols
{
    public abstract class IntrinsicNumericTypeSymbol : IntrinsicTypeSymbol
    {
        public ScalarType ScalarType { get; }

        internal IntrinsicNumericTypeSymbol(SymbolKind kind, string name, string documentation, ScalarType scalarType)
            : base(kind, name, documentation)
        {
            ScalarType = scalarType;
        }

        protected bool Equals(IntrinsicNumericTypeSymbol other)
        {
            return base.Equals(other) && ScalarType == other.ScalarType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IntrinsicNumericTypeSymbol) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (int) ScalarType;
            }
        }
    }
}