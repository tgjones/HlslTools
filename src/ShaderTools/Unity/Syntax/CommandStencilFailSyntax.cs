namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandStencilFailSyntax : CommandSyntax
    {
        public readonly SyntaxToken FailKeyword;
        public readonly CommandValueSyntax Value;

        public CommandStencilFailSyntax(SyntaxToken failKeyword, CommandValueSyntax value)
            : base(SyntaxKind.CommandStencilFail)
        {
            RegisterChildNode(out FailKeyword, failKeyword);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandStencilFail(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandStencilFail(this);
        }
    }
}