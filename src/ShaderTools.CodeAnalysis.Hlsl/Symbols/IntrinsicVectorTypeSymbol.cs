using ShaderTools.CodeAnalysis.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public sealed class IntrinsicVectorTypeSymbol : IntrinsicNumericTypeSymbol
    {
        internal IntrinsicVectorTypeSymbol(string name, string documentation, ScalarType scalarType, int numComponents)
            : base(SymbolKind.IntrinsicVectorType, name, documentation, scalarType)
        {
            NumComponents = numComponents;
        }

        public int NumComponents { get; }

        private bool Equals(IntrinsicVectorTypeSymbol other)
        {
            return base.Equals(other) && NumComponents == other.NumComponents;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is IntrinsicVectorTypeSymbol && Equals((IntrinsicVectorTypeSymbol) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ NumComponents;
            }
        }
    }
}