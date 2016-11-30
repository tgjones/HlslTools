namespace ShaderTools.Unity.Syntax
{
    public abstract class CommandSyntax : SyntaxNode
    {
        protected CommandSyntax(SyntaxKind kind)
            : base(kind)
        {
        }
    }
}