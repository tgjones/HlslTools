namespace ShaderTools.Unity.Syntax
{
    public sealed class ShaderPropertyAttributeSyntax : SyntaxNode
    {
        public readonly SyntaxToken OpenBracketToken;
        public readonly SyntaxToken Name;
        public readonly AttributeArgumentListSyntax ArgumentList;
        public readonly SyntaxToken CloseBracketToken;

        public ShaderPropertyAttributeSyntax(SyntaxToken openBracketToken, SyntaxToken name, AttributeArgumentListSyntax argumentList, SyntaxToken closeBracketToken)
            : base(SyntaxKind.ShaderPropertyAttribute)
        {
            RegisterChildNode(out OpenBracketToken, openBracketToken);
            RegisterChildNode(out Name, name);
            RegisterChildNode(out ArgumentList, argumentList);
            RegisterChildNode(out CloseBracketToken, closeBracketToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitShaderPropertyAttribute(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitShaderPropertyAttribute(this);
        }
    }
}