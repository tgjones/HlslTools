namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public abstract class NumericTypeSyntax : PredefinedTypeSyntax
    {
        protected NumericTypeSyntax(SyntaxKind kind)
            : base(kind)
        {
        }
    }
}