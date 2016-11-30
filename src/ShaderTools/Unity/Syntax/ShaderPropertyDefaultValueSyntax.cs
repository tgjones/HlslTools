namespace ShaderTools.Unity.Syntax
{
    public abstract class ShaderPropertyDefaultValueSyntax : SyntaxNode
    {
        protected ShaderPropertyDefaultValueSyntax(SyntaxKind kind)
            : base(kind)
        {
        }
    }
}