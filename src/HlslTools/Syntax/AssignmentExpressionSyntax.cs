namespace HlslTools.Syntax
{
    public class AssignmentExpressionSyntax : ExpressionSyntax
    {
        public readonly ExpressionSyntax Left;
        public readonly SyntaxToken OperatorToken;
        public readonly SyntaxToken LessThanToken;
        public readonly ExpressionSyntax Right;
        public readonly SyntaxToken GreaterThanToken;

        public AssignmentExpressionSyntax(SyntaxKind kind, ExpressionSyntax left, SyntaxToken operatorToken, SyntaxToken lessThanToken, ExpressionSyntax right, SyntaxToken greaterThanToken)
            : base(kind)
        {
            RegisterChildNode(out Left, left);
            RegisterChildNode(out OperatorToken, operatorToken);
            RegisterChildNode(out LessThanToken, lessThanToken);
            RegisterChildNode(out Right, right);
            RegisterChildNode(out GreaterThanToken, greaterThanToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitAssignmentExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitAssignmentExpression(this);
        }
    }
}