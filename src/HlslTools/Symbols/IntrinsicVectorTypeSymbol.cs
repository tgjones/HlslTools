namespace HlslTools.Symbols
{
    public sealed class IntrinsicVectorTypeSymbol : IntrinsicNumericTypeSymbol
    {
        internal IntrinsicVectorTypeSymbol(string name, string documentation, ScalarType scalarType, int numComponents)
            : base(SymbolKind.IntrinsicVectorType, name, documentation, scalarType)
        {
            NumComponents = numComponents;
        }

        public int NumComponents { get; }
    }
}