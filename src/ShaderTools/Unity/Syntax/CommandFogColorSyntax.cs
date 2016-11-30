namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandFogColorSyntax : CommandSyntax
    {
        public readonly SyntaxToken ColorKeyword;
        public readonly CommandValueSyntax Value;

        public CommandFogColorSyntax(SyntaxToken colorKeyword, CommandValueSyntax value)
            : base(SyntaxKind.CommandFogColor)
        {
            RegisterChildNode(out ColorKeyword, colorKeyword);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandFogColor(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandFogColor(this);
        }
    }
}
