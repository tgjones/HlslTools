namespace ShaderTools.Unity.Syntax
{
    public sealed class CgProgramSyntax : SyntaxNode
    {
        public readonly SyntaxToken CgProgramKeyword;
        public readonly SyntaxToken EndCgKeyword;

        public CgProgramSyntax(SyntaxToken cgProgramKeyword, SyntaxToken endCgKeyword)
            : base(SyntaxKind.CgProgram)
        {
            RegisterChildNode(out CgProgramKeyword, cgProgramKeyword);
            RegisterChildNode(out EndCgKeyword, endCgKeyword);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCgProgram(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCgProgram(this);
        }
    }
}