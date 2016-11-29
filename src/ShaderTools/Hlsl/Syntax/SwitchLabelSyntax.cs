namespace ShaderTools.Hlsl.Syntax
{
    public abstract class SwitchLabelSyntax : SyntaxNode
    {
        public abstract SyntaxToken Keyword { get; }
        public abstract SyntaxToken ColonToken { get; }

        protected SwitchLabelSyntax(SyntaxKind kind)
            : base(kind)
        {
        }
    }
}