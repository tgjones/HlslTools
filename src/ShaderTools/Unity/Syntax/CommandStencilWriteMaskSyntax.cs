namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandStencilWriteMaskSyntax : CommandSyntax
    {
        public readonly SyntaxToken WriteMaskKeyword;
        public readonly CommandValueSyntax Value;

        public CommandStencilWriteMaskSyntax(SyntaxToken writeMaskKeyword, CommandValueSyntax value)
            : base(SyntaxKind.CommandStencilWriteMask)
        {
            RegisterChildNode(out WriteMaskKeyword, writeMaskKeyword);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandStencilWriteMask(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandStencilWriteMask(this);
        }
    }
}