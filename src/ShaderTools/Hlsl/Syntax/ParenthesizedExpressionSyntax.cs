namespace ShaderTools.Hlsl.Syntax
{
    public class ParenthesizedExpressionSyntax : ExpressionSyntax
    {
        public readonly SyntaxToken OpenParenToken;
        public readonly ExpressionSyntax Expression;
        public readonly SyntaxToken CloseParenToken;

        public ParenthesizedExpressionSyntax(SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken)
            : base(SyntaxKind.ParenthesizedExpression)
        {
            RegisterChildNode(out OpenParenToken, openParenToken);
            RegisterChildNode(out Expression, expression);
            RegisterChildNode(out CloseParenToken, closeParenToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitParenthesizedExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitParenthesizedExpression(this);
        }
    }
}