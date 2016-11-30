namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandStencilCompSyntax : CommandSyntax
    {
        public readonly SyntaxToken CompKeyword;
        public readonly CommandValueSyntax Value;

        public CommandStencilCompSyntax(SyntaxToken compKeyword, CommandValueSyntax value)
            : base(SyntaxKind.CommandStencilComp)
        {
            RegisterChildNode(out CompKeyword, compKeyword);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandStencilComp(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandStencilComp(this);
        }
    }
}