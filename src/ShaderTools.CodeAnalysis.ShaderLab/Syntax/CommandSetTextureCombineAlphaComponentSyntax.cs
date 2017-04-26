namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class CommandSetTextureCombineAlphaComponentSyntax : SyntaxNode
    {
        public readonly SyntaxToken CommaToken;
        public readonly BaseCommandSetTextureCombineValueSyntax Value;

        public CommandSetTextureCombineAlphaComponentSyntax(SyntaxToken commaToken, BaseCommandSetTextureCombineValueSyntax value)
            : base(SyntaxKind.CommandSetTextureCombineAlphaComponent)
        {
            RegisterChildNode(out CommaToken, commaToken);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandSetTextureCombineAlphaComponent(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandSetTextureCombineAlphaComponent(this);
        }
    }
}