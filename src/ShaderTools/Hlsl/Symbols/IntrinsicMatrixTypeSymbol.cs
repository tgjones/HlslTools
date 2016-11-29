namespace ShaderTools.Hlsl.Symbols
{
    public sealed class IntrinsicMatrixTypeSymbol : IntrinsicNumericTypeSymbol
    {
        internal IntrinsicMatrixTypeSymbol(string name, string documentation, ScalarType scalarType, int rows, int cols)
            : base(SymbolKind.IntrinsicMatrixType, name, documentation, scalarType)
        {
            Rows = rows;
            Cols = cols;
        }
        
        public int Rows { get; }
        public int Cols { get; }

        private bool Equals(IntrinsicMatrixTypeSymbol other)
        {
            return base.Equals(other) && Rows == other.Rows && Cols == other.Cols;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is IntrinsicMatrixTypeSymbol && Equals((IntrinsicMatrixTypeSymbol) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ Rows;
                hashCode = (hashCode * 397) ^ Cols;
                return hashCode;
            }
        }
    }
}