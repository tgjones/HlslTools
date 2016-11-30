namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandCullSyntax : CommandSyntax
    {
        public readonly SyntaxToken CullKeyword;
        public readonly CommandValueSyntax Value;

        public CommandCullSyntax(SyntaxToken cullKeyword, CommandValueSyntax value)
            : base(SyntaxKind.CommandCull)
        {
            RegisterChildNode(out CullKeyword, cullKeyword);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandCull(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandCull(this);
        }
    }
}