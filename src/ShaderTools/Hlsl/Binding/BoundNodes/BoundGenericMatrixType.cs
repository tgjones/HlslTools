using ShaderTools.Hlsl.Symbols;

namespace ShaderTools.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundGenericMatrixType : BoundType
    {
        public IntrinsicMatrixTypeSymbol MatrixSymbol { get; }
        public BoundScalarType ScalarType { get; }

        public BoundGenericMatrixType(IntrinsicMatrixTypeSymbol matrixSymbol, BoundScalarType scalarType)
            : base(BoundNodeKind.IntrinsicGenericMatrixType, matrixSymbol)
        {
            MatrixSymbol = matrixSymbol;
            ScalarType = scalarType;
        }
    }
}