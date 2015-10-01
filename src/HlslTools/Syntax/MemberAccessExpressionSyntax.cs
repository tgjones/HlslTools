namespace HlslTools.Syntax
{
    public sealed class MemberAccessExpressionSyntax : ExpressionSyntax
    {
        public readonly ExpressionSyntax Expression;
        public readonly SyntaxToken DotToken;
        public readonly IdentifierNameSyntax Name;

        public MemberAccessExpressionSyntax(ExpressionSyntax expression, SyntaxToken dotToken, IdentifierNameSyntax name)
            : base(SyntaxKind.MemberAccessExpression)
        {
            RegisterChildNode(out Expression, expression);
            RegisterChildNode(out DotToken, dotToken);
            RegisterChildNode(out Name, name);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitMemberAccess(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitMemberAccess(this);
        }
    }
}