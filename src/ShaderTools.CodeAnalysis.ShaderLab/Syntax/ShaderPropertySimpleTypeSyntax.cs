namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class ShaderPropertySimpleTypeSyntax : ShaderPropertyTypeSyntax
    {
        public readonly SyntaxToken TypeKeyword;

        public override SyntaxKind TypeKind => TypeKeyword.Kind;

        public ShaderPropertySimpleTypeSyntax(SyntaxToken typeKeyword)
            : base(SyntaxKind.ShaderPropertySimpleType)
        {
            RegisterChildNode(out TypeKeyword, typeKeyword);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitShaderPropertySimpleType(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitShaderPropertySimpleType(this);
        }
    }
}