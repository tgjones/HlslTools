namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class AttributeSyntax : SyntaxNode
    {
        public readonly SyntaxToken OpenBracketToken;
        public readonly IdentifierNameSyntax Name;
        public readonly AttributeArgumentListSyntax ArgumentList;
        public readonly SyntaxToken CloseBracketToken;

        public AttributeSyntax(SyntaxToken openBracketToken, IdentifierNameSyntax name, AttributeArgumentListSyntax argumentList, SyntaxToken closeBracketToken)
            : base(SyntaxKind.Attribute)
        {
            RegisterChildNode(out OpenBracketToken, openBracketToken);
            RegisterChildNode(out Name, name);
            RegisterChildNode(out ArgumentList, argumentList);
            RegisterChildNode(out CloseBracketToken, closeBracketToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitAttribute(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitAttribute(this);
        }
    }
}