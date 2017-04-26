namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public abstract class ShaderPropertyTypeSyntax : SyntaxNode
    {
        public abstract SyntaxKind TypeKind { get; }

        protected ShaderPropertyTypeSyntax(SyntaxKind kind)
            : base(kind)
        {
        }
    }
}