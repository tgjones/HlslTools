namespace HlslTools.Symbols
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
    }
}