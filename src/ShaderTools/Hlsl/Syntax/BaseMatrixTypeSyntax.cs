namespace ShaderTools.Hlsl.Syntax
{
    public abstract class BaseMatrixTypeSyntax : NumericTypeSyntax
    {
        protected BaseMatrixTypeSyntax(SyntaxKind kind)
            : base(kind)
        {
        }
    }
}