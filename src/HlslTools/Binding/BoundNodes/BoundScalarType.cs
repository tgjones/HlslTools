using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
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