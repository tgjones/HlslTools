namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandLightingSyntax : CommandSyntax
    {
        public readonly SyntaxToken LightingKeyword;
        public readonly SyntaxToken ValueToken;

        public CommandLightingSyntax(SyntaxToken lightingKeyword, SyntaxToken valueToken)
            : base(SyntaxKind.CommandLighting)
        {
            RegisterChildNode(out LightingKeyword, lightingKeyword);
            RegisterChildNode(out ValueToken, valueToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandLighting(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandLighting(this);
        }
    }
}