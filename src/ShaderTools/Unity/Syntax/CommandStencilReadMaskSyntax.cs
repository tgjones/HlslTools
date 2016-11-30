namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandStencilReadMaskSyntax : CommandSyntax
    {
        public readonly SyntaxToken ReadMaskKeyword;
        public readonly CommandValueSyntax Value;

        public CommandStencilReadMaskSyntax(SyntaxToken readMaskKeyword, CommandValueSyntax value)
            : base(SyntaxKind.CommandStencilReadMask)
        {
            RegisterChildNode(out ReadMaskKeyword, readMaskKeyword);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandStencilReadMask(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandStencilReadMask(this);
        }
    }
}