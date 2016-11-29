namespace ShaderTools.Hlsl.Syntax
{
    public class StatePropertySyntax : SyntaxNode
    {
        public readonly SyntaxToken Name;
        public readonly ArrayRankSpecifierSyntax ArrayRankSpecifier;
        public readonly SyntaxToken EqualsToken;
        public readonly SyntaxToken LessThanToken;
        public readonly ExpressionSyntax Value;
        public readonly SyntaxToken GreaterThanToken;
        public readonly SyntaxToken SemicolonToken;

        public StatePropertySyntax(SyntaxToken name, ArrayRankSpecifierSyntax arrayRankSpecifier, SyntaxToken equalsToken, SyntaxToken lessThanToken, ExpressionSyntax value, SyntaxToken greaterThanToken, SyntaxToken semicolonToken)
            : base(SyntaxKind.StateProperty)
        {
            RegisterChildNode(out Name, name);
            RegisterChildNode(out ArrayRankSpecifier, arrayRankSpecifier);
            RegisterChildNode(out EqualsToken, equalsToken);
            RegisterChildNode(out LessThanToken, lessThanToken);
            RegisterChildNode(out Value, value);
            RegisterChildNode(out GreaterThanToken, greaterThanToken);
            RegisterChildNode(out SemicolonToken, semicolonToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitStateProperty(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitStateProperty(this);
        }
    }
}