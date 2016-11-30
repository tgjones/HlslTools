namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandSetTextureCombineSyntax : CommandSyntax
    {
        public readonly SyntaxToken CombineKeyword;
        public readonly BaseCommandSetTextureCombineValueSyntax Value;
        public readonly SyntaxToken Modifier;
        public readonly CommandSetTextureCombineAlphaComponentSyntax AlphaComponent;

        public CommandSetTextureCombineSyntax(SyntaxToken combineKeyword, BaseCommandSetTextureCombineValueSyntax value, SyntaxToken modifier, CommandSetTextureCombineAlphaComponentSyntax alphaComponent)
            : base(SyntaxKind.CommandSetTextureCombine)
        {
            RegisterChildNode(out CombineKeyword, combineKeyword);
            RegisterChildNode(out Value, value);
            RegisterChildNode(out Modifier, modifier);
            RegisterChildNode(out AlphaComponent, alphaComponent);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandSetTextureCombine(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandSetTextureCombine(this);
        }
    }
}