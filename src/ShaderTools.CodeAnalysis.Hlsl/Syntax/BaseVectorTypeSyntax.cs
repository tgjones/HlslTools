namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public abstract class BaseVectorTypeSyntax : NumericTypeSyntax
    {
        protected BaseVectorTypeSyntax(SyntaxKind kind)
            : base(kind)
        {
        }
    }
}