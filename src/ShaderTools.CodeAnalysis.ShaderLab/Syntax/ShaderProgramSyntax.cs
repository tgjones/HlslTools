namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class ShaderProgramSyntax : SyntaxNode
    {
        public readonly SyntaxToken BeginProgramKeyword;
        public readonly SyntaxToken EndProgramKeyword;

        public ShaderProgramSyntax(SyntaxKind kind, SyntaxToken beginProgramKeyword, SyntaxToken endProgramKeyword)
            : base(kind)
        {
            RegisterChildNode(out BeginProgramKeyword, beginProgramKeyword);
            RegisterChildNode(out EndProgramKeyword, endProgramKeyword);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitShaderProgram(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitShaderProgram(this);
        }
    }
}