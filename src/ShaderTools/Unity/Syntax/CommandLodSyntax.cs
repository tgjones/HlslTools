namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandLodSyntax : CommandSyntax
    {
        public readonly SyntaxToken LodKeyword;
        public readonly CommandValueSyntax Value;

        public CommandLodSyntax(SyntaxToken lodKeyword, CommandValueSyntax value)
            : base(SyntaxKind.CommandLod)
        {
            RegisterChildNode(out LodKeyword, lodKeyword);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandLod(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandLod(this);
        }
    }
}