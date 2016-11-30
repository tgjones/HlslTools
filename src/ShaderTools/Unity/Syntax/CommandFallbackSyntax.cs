namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandFallbackSyntax : CommandSyntax
    {
        public readonly SyntaxToken FallbackKeyword;
        public readonly SyntaxToken ValueToken;

        public CommandFallbackSyntax(SyntaxToken fallbackKeyword, SyntaxToken valueToken)
            : base(SyntaxKind.CommandFallback)
        {
            RegisterChildNode(out FallbackKeyword, fallbackKeyword);
            RegisterChildNode(out ValueToken, valueToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandFallback(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandFallback(this);
        }
    }
}