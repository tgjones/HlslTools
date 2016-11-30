namespace ShaderTools.Unity.Syntax
{
    public abstract class CommandValueSyntax : SyntaxNode
    {
        protected CommandValueSyntax(SyntaxKind kind)
            : base(kind)
        {
        }
    }
}