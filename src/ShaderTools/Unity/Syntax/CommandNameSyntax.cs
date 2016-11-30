namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandNameSyntax : CommandSyntax
    {
        public readonly SyntaxToken NameKeyword;
        public readonly SyntaxToken NameToken;

        public CommandNameSyntax(SyntaxToken nameKeyword, SyntaxToken nameToken)
            : base(SyntaxKind.CommandName)
        {
            RegisterChildNode(out NameKeyword, nameKeyword);
            RegisterChildNode(out NameToken, nameToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandName(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandName(this);
        }
    }
}