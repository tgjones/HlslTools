using ShaderTools.Hlsl.Symbols;

namespace ShaderTools.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundMatrixType : BoundType
    {
        public IntrinsicMatrixTypeSymbol MatrixSymbol { get; }

        public BoundMatrixType(IntrinsicMatrixTypeSymbol matrixSymbol)
            : base(BoundNodeKind.IntrinsicMatrixType, matrixSymbol)
        {
            MatrixSymbol = matrixSymbol;
        }
    }
}