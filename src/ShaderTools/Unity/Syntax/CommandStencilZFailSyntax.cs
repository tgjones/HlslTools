namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandStencilZFailSyntax : CommandSyntax
    {
        public readonly SyntaxToken ZFailKeyword;
        public readonly CommandValueSyntax Value;

        public CommandStencilZFailSyntax(SyntaxToken zFailKeyword, CommandValueSyntax value)
            : base(SyntaxKind.CommandStencilZFail)
        {
            RegisterChildNode(out ZFailKeyword, zFailKeyword);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandStencilZFail(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandStencilZFail(this);
        }
    }
}