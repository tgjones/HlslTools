namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandFogDensitySyntax : CommandSyntax
    {
        public readonly SyntaxToken DensityKeyword;
        public readonly CommandValueSyntax Value;

        public CommandFogDensitySyntax(SyntaxToken densityKeyword, CommandValueSyntax value)
            : base(SyntaxKind.CommandFogDensity)
        {
            RegisterChildNode(out DensityKeyword, densityKeyword);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandFogDensity(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandFogDensity(this);
        }
    }
}
