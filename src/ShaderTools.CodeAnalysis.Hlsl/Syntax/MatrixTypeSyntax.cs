namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class MatrixTypeSyntax : BaseMatrixTypeSyntax
    {
        public readonly SyntaxToken TypeToken;

        public MatrixTypeSyntax(SyntaxToken typeToken)
            : base(SyntaxKind.PredefinedMatrixType)
        {
            RegisterChildNode(out TypeToken, typeToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitMatrixType(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitMatrixType(this);
        }
    }
}