namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    /// <summary>
    /// ? : construct
    /// </summary>
    public class ConditionalExpressionSyntax : ExpressionSyntax
    {
        public readonly ExpressionSyntax Condition;
        public readonly SyntaxToken QuestionToken;
        public readonly ExpressionSyntax WhenTrue;
        public readonly SyntaxToken ColonToken;
        public readonly ExpressionSyntax WhenFalse;

        public ConditionalExpressionSyntax(ExpressionSyntax condition, SyntaxToken questionToken, ExpressionSyntax whenTrue, SyntaxToken colonToken, ExpressionSyntax whenFalse)
            : base(SyntaxKind.ConditionalExpression)
        {
            RegisterChildNode(out Condition, condition);
            RegisterChildNode(out QuestionToken, questionToken);
            RegisterChildNode(out WhenTrue, whenTrue);
            RegisterChildNode(out ColonToken, colonToken);
            RegisterChildNode(out WhenFalse, whenFalse);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitConditionalExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitConditionalExpression(this);
        }
    }
}