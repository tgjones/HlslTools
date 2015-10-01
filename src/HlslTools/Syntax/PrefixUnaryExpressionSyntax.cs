namespace HlslTools.Syntax
{
    public class PrefixUnaryExpressionSyntax : ExpressionSyntax
    {
        public readonly SyntaxToken OperatorToken;
        public readonly ExpressionSyntax Operand;

        public PrefixUnaryExpressionSyntax(SyntaxKind kind, SyntaxToken operatorToken, ExpressionSyntax operand)
            : base(kind)
        {
            RegisterChildNode(out OperatorToken, operatorToken);
            RegisterChildNode(out Operand, operand);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitPrefixUnaryExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitPrefixUnaryExpression(this);
        }
    }
}