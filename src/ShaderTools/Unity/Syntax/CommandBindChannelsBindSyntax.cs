namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandBindChannelsBindSyntax : CommandSyntax
    {
        public readonly SyntaxToken BindKeyword;
        public readonly SyntaxToken SourceToken;
        public readonly SyntaxToken CommaToken;
        public readonly SyntaxToken TargetToken;

        public CommandBindChannelsBindSyntax(SyntaxToken bindKeyword, SyntaxToken sourceToken, SyntaxToken commaToken, SyntaxToken targetToken)
            : base(SyntaxKind.CommandBindChannelsBind)
        {
            RegisterChildNode(out BindKeyword, bindKeyword);
            RegisterChildNode(out SourceToken, sourceToken);
            RegisterChildNode(out CommaToken, commaToken);
            RegisterChildNode(out TargetToken, targetToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandBindChannelsBind(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandBindChannelsBind(this);
        }
    }
}