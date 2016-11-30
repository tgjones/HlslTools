namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandConstantValueSyntax : CommandValueSyntax
    {
        public readonly SyntaxToken ValueToken;

        public CommandConstantValueSyntax(SyntaxToken valueToken)
            : base(SyntaxKind.CommandConstantValue)
        {
            RegisterChildNode(out ValueToken, valueToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandConstantValue(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandConstantValue(this);
        }
    }
}