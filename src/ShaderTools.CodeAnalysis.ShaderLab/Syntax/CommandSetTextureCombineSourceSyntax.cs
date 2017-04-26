namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class CommandSetTextureCombineSourceSyntax : SyntaxNode
    {
        public readonly SyntaxToken SourceToken;
        public readonly SyntaxToken AlphaKeyword;

        public CommandSetTextureCombineSourceSyntax(SyntaxToken sourceToken, SyntaxToken alphaKeyword)
            : base(SyntaxKind.CommandSetTextureCombineSource)
        {
            RegisterChildNode(out SourceToken, sourceToken);
            RegisterChildNode(out AlphaKeyword, alphaKeyword);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandSetTextureCombineSource(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandSetTextureCombineSource(this);
        }
    }
}