namespace ShaderTools.Unity.Syntax
{
    public class CompilationUnitSyntax : SyntaxNode
    {
        public readonly ShaderSyntax Shader;
        public readonly SyntaxToken EndOfFileToken;

        public CompilationUnitSyntax(ShaderSyntax shader, SyntaxToken endOfFileToken)
            : base(SyntaxKind.CompilationUnit)
        {
            RegisterChildNode(out Shader, shader);
            RegisterChildNode(out EndOfFileToken, endOfFileToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCompilationUnit(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCompilationUnit(this);
        }
    }
}