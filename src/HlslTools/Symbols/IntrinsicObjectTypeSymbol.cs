using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public sealed class IntrinsicObjectTypeSymbol : IntrinsicTypeSymbol
    {
        public PredefinedObjectType PredefinedType { get; }

        internal IntrinsicObjectTypeSymbol(string name, string documentation, PredefinedObjectType predefinedType)
            : base(SymbolKind.IntrinsicObjectType, name, documentation)
        {
            PredefinedType = predefinedType;
        }

        private bool Equals(IntrinsicObjectTypeSymbol other)
        {
            return base.Equals(other) && PredefinedType == other.PredefinedType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is IntrinsicObjectTypeSymbol && Equals((IntrinsicObjectTypeSymbol) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (int) PredefinedType;
            }
        }
    }
}