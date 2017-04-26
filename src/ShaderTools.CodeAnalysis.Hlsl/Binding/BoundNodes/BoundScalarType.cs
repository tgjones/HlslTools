using ShaderTools.CodeAnalysis.Hlsl.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundScalarType : BoundType
    {
        public IntrinsicScalarTypeSymbol ScalarSymbol { get; }

        public BoundScalarType(IntrinsicScalarTypeSymbol scalarSymbol)
            : base(BoundNodeKind.IntrinsicScalarType, scalarSymbol)
        {
            ScalarSymbol = scalarSymbol;
        }
    }
}