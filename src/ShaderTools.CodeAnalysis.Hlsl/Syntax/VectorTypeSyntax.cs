namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class VectorTypeSyntax : BaseVectorTypeSyntax
    {
        public readonly SyntaxToken TypeToken;

        public VectorTypeSyntax(SyntaxToken typeToken)
            : base(SyntaxKind.PredefinedVectorType)
        {
            RegisterChildNode(out TypeToken, typeToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitVectorType(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitVectorType(this);
        }
    }
}