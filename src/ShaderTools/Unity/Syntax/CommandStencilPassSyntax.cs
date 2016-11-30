namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandStencilPassSyntax : CommandSyntax
    {
        public readonly SyntaxToken PassKeyword;
        public readonly CommandValueSyntax Value;

        public CommandStencilPassSyntax(SyntaxToken passKeyword, CommandValueSyntax value)
            : base(SyntaxKind.CommandStencilPass)
        {
            RegisterChildNode(out PassKeyword, passKeyword);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandStencilPass(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandStencilPass(this);
        }
    }
}