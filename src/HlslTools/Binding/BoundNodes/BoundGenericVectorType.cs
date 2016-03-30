using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundGenericVectorType : BoundType
    {
        public IntrinsicVectorTypeSymbol VectorSymbol { get; }
        public BoundScalarType ScalarType { get; }

        public BoundGenericVectorType(IntrinsicVectorTypeSymbol vectorSymbol, BoundScalarType scalarType)
            : base(BoundNodeKind.IntrinsicGenericVectorType, vectorSymbol)
        {
            VectorSymbol = vectorSymbol;
            ScalarType = scalarType;
        }
    }
}