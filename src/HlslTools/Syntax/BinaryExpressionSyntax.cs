using HlslTools.Binding.BoundNodes;

namespace HlslTools.Syntax
{
    public class BinaryExpressionSyntax : ExpressionSyntax
    {
        public readonly ExpressionSyntax Left;
        public readonly SyntaxToken OperatorToken;
        public readonly BinaryOperatorKind Operator;
        public readonly ExpressionSyntax Right;

        public BinaryExpressionSyntax(SyntaxKind kind, ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
            : base(kind)
        {
            RegisterChildNode(out Left, left);
            RegisterChildNode(out OperatorToken, operatorToken);
            Operator = SyntaxFacts.GetBinaryOperatorKind(kind);
            RegisterChildNode(out Right, right);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitBinaryExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitBinaryExpression(this);
        }
    }
}