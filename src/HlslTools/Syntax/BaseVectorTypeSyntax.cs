namespace HlslTools.Syntax
{
    public abstract class BaseVectorTypeSyntax : NumericTypeSyntax
    {
        protected BaseVectorTypeSyntax(SyntaxKind kind)
            : base(kind)
        {
        }
    }
}