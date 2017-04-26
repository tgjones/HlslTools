namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class ArrayRankSpecifierSyntax : SyntaxNode
    {
        public readonly SyntaxToken OpenBracketToken;
        public readonly ExpressionSyntax Dimension;
        public readonly SyntaxToken CloseBracketToken;

        public ArrayRankSpecifierSyntax(SyntaxToken openBracketToken, ExpressionSyntax dimension, SyntaxToken closeBracketToken)
            : base(SyntaxKind.ArrayRankSpecifier)
        {
            RegisterChildNode(out OpenBracketToken, openBracketToken);
            RegisterChildNode(out Dimension, dimension);
            RegisterChildNode(out CloseBracketToken, closeBracketToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitArrayRankSpecifier(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitArrayRankSpecifier(this);
        }
    }
}