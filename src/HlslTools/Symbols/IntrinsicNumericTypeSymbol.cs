namespace HlslTools.Symbols
{
    public abstract class IntrinsicNumericTypeSymbol : IntrinsicTypeSymbol
    {
        public ScalarType ScalarType { get; }

        protected IntrinsicNumericTypeSymbol(SymbolKind kind, string name, string documentation, ScalarType scalarType)
            : base(kind, name, documentation)
        {
            ScalarType = scalarType;
        }
    }
}