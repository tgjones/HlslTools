using ShaderTools.CodeAnalysis.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public sealed class IntrinsicScalarTypeSymbol : IntrinsicNumericTypeSymbol
    {
        internal IntrinsicScalarTypeSymbol(string name, string documentation, ScalarType scalarType)
            : base(SymbolKind.IntrinsicScalarType, name, documentation, scalarType)
        {
            
        }
    }
}