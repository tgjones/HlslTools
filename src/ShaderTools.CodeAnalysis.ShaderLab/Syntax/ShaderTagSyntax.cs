namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class ShaderTagSyntax : SyntaxNode
    {
        public readonly SyntaxToken NameToken;
        public readonly SyntaxToken EqualsToken;
        public readonly SyntaxToken ValueToken;

        public ShaderTagSyntax(SyntaxToken nameToken, SyntaxToken equalsToken, SyntaxToken valueToken)
            : base(SyntaxKind.ShaderTag)
        {
            RegisterChildNode(out NameToken, nameToken);
            RegisterChildNode(out EqualsToken, equalsToken);
            RegisterChildNode(out ValueToken, valueToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitShaderTag(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitShaderTag(this);
        }
    }
}