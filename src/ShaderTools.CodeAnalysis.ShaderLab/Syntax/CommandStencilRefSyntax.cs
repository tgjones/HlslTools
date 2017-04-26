namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class CommandStencilRefSyntax : CommandSyntax
    {
        public readonly SyntaxToken RefKeyword;
        public readonly CommandValueSyntax Value;

        public CommandStencilRefSyntax(SyntaxToken refKeyword, CommandValueSyntax value)
            : base(SyntaxKind.CommandStencilRef)
        {
            RegisterChildNode(out RefKeyword, refKeyword);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandStencilRef(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandStencilRef(this);
        }
    }
}