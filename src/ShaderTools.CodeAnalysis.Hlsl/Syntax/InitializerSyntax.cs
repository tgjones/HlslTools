namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public abstract class InitializerSyntax : SyntaxNode
    {
        protected InitializerSyntax(SyntaxKind kind)
            : base(kind)
        {
        }
    }
}