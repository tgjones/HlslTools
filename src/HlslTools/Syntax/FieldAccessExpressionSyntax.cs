namespace HlslTools.Syntax
{
    public sealed class FieldAccessExpressionSyntax : ExpressionSyntax
    {
        public readonly ExpressionSyntax Expression;
        public readonly SyntaxToken DotToken;
        public readonly SyntaxToken Name;

        public FieldAccessExpressionSyntax(ExpressionSyntax expression, SyntaxToken dotToken, SyntaxToken name)
            : base(SyntaxKind.FieldAccessExpression)
        {
            RegisterChildNode(out Expression, expression);
            RegisterChildNode(out DotToken, dotToken);
            RegisterChildNode(out Name, name);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitFieldAccess(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitFieldAccess(this);
        }
    }
}