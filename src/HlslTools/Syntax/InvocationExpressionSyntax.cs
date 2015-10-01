namespace HlslTools.Syntax
{
    public class InvocationExpressionSyntax : ExpressionSyntax
    {
        public readonly ExpressionSyntax Expression;
        public readonly ArgumentListSyntax ArgumentList;

        public InvocationExpressionSyntax(ExpressionSyntax expression, ArgumentListSyntax argumentList)
            : base(SyntaxKind.InvocationExpression)
        {
            RegisterChildNode(out Expression, expression);
            RegisterChildNode(out ArgumentList, argumentList);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitInvocationExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitInvocationExpression(this);
        }
    }
}