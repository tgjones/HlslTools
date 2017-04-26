namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class CommandFogModeSyntax : CommandSyntax
    {
        public readonly SyntaxToken ModeKeyword;
        public readonly CommandValueSyntax Value;

        public CommandFogModeSyntax(SyntaxToken modeKeyword, CommandValueSyntax value)
            : base(SyntaxKind.CommandFogMode)
        {
            RegisterChildNode(out ModeKeyword, modeKeyword);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandFogMode(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandFogMode(this);
        }
    }
}
