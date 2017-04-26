namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public abstract class CommandValueSyntax : SyntaxNode
    {
        protected CommandValueSyntax(SyntaxKind kind)
            : base(kind)
        {
        }
    }
}