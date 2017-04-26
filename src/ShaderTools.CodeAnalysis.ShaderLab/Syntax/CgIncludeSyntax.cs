namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class CgIncludeSyntax : SyntaxNode
    {
        public readonly SyntaxToken CgIncludeKeyword;
        public readonly SyntaxToken EndCgKeyword;

        public CgIncludeSyntax(SyntaxToken cgIncludeKeyword, SyntaxToken endCgKeyword)
            : base(SyntaxKind.CgInclude)
        {
            RegisterChildNode(out CgIncludeKeyword, cgIncludeKeyword);
            RegisterChildNode(out EndCgKeyword, endCgKeyword);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCgInclude(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCgInclude(this);
        }
    }
}