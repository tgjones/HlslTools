namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public abstract class PredefinedTypeSyntax : TypeSyntax
    {
        protected PredefinedTypeSyntax(SyntaxKind kind)
            : base(kind)
        {
        }
    }
}