namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public abstract class CommandSyntax : SyntaxNode
    {
        protected CommandSyntax(SyntaxKind kind)
            : base(kind)
        {
        }
    }
}