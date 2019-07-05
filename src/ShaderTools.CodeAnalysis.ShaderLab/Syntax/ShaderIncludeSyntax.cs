namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class ShaderIncludeSyntax : SyntaxNode
    {
        public readonly SyntaxToken BeginIncludeKeyword;
        public readonly SyntaxToken EndIncludeKeyword;

        public ShaderIncludeSyntax(SyntaxKind kind, SyntaxToken beginIncludeKeyword, SyntaxToken endIncludeKeyword)
            : base(kind)
        {
            RegisterChildNode(out BeginIncludeKeyword, beginIncludeKeyword);
            RegisterChildNode(out EndIncludeKeyword, endIncludeKeyword);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitShaderInclude(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitShaderInclude(this);
        }
    }
}