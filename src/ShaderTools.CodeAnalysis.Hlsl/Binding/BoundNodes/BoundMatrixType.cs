using ShaderTools.CodeAnalysis.Hlsl.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
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