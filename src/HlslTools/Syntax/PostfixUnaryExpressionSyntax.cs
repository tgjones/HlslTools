using HlslTools.Binding;
using HlslTools.Binding.BoundNodes;

namespace HlslTools.Syntax
{
    public class PostfixUnaryExpressionSyntax : ExpressionSyntax
    {
        public readonly ExpressionSyntax Operand;
        public readonly SyntaxToken OperatorToken;
        public readonly UnaryOperatorKind Operator;

        public PostfixUnaryExpressionSyntax(SyntaxKind kind, ExpressionSyntax operand, SyntaxToken operatorToken)
            : base(kind)
        {
            RegisterChildNode(out Operand, operand);
            RegisterChildNode(out OperatorToken, operatorToken);
            Operator = SyntaxFacts.GetUnaryOperatorKind(kind);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitPostfixUnaryExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitPostfixUnaryExpression(this);
        }
    }
}