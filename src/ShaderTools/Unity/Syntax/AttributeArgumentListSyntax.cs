namespace ShaderTools.Unity.Syntax
{
    public class AttributeArgumentListSyntax : SyntaxNode
    {
        public readonly SyntaxToken OpenParenToken;
        public readonly SeparatedSyntaxList<LiteralExpressionSyntax> Arguments;
        public readonly SyntaxToken CloseParenToken;

        public AttributeArgumentListSyntax(SyntaxToken openParenToken, SeparatedSyntaxList<LiteralExpressionSyntax> arguments, SyntaxToken closeParenToken)
            : base(SyntaxKind.Attribute)
        {
            RegisterChildNode(out OpenParenToken, openParenToken);
            RegisterChildNodes(out Arguments, arguments);
            RegisterChildNode(out CloseParenToken, closeParenToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitAttributeArgumentList(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitAttributeArgumentList(this);
        }
    }
}