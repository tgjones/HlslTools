namespace HlslTools.Syntax
{
    public sealed class EqualsValueClauseSyntax : InitializerSyntax
    {
        public readonly SyntaxToken EqualsToken;
        public readonly ExpressionSyntax Value;

        public EqualsValueClauseSyntax(SyntaxToken equalsToken, ExpressionSyntax value)
            : base(SyntaxKind.EqualsValueClause)
        {
            RegisterChildNode(out EqualsToken, equalsToken);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitEqualsValueClause(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitEqualsValueClause(this);
        }
    }
}