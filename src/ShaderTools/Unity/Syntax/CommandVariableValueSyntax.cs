namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandVariableValueSyntax : CommandValueSyntax
    {
        public readonly SyntaxToken OpenBracketToken;
        public readonly SyntaxToken Identifier;
        public readonly SyntaxToken CloseBracketToken;

        public CommandVariableValueSyntax(SyntaxToken openBracketToken, SyntaxToken identifier, SyntaxToken closeBracketToken)
            : base(SyntaxKind.CommandVariableValue)
        {
            RegisterChildNode(out OpenBracketToken, openBracketToken);
            RegisterChildNode(out Identifier, identifier);
            RegisterChildNode(out CloseBracketToken, closeBracketToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandVariableValue(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandVariableValue(this);
        }
    }
}