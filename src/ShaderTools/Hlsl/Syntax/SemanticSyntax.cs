namespace ShaderTools.Hlsl.Syntax
{
    public class SemanticSyntax : VariableDeclaratorQualifierSyntax
    {
        public readonly SyntaxToken ColonToken;
        public readonly SyntaxToken Semantic;

        public SemanticSyntax(SyntaxToken colonToken, SyntaxToken semantic)
            : base(SyntaxKind.SemanticName)
        {
            RegisterChildNode(out ColonToken, colonToken);
            RegisterChildNode(out Semantic, semantic);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitSemantic(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitSemantic(this);
        }
    }
}