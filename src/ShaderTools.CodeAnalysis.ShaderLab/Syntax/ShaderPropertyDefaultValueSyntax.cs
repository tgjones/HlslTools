namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public abstract class ShaderPropertyDefaultValueSyntax : SyntaxNode
    {
        protected ShaderPropertyDefaultValueSyntax(SyntaxKind kind)
            : base(kind)
        {
        }
    }
}