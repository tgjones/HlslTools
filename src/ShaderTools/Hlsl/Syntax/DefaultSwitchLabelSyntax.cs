namespace ShaderTools.Hlsl.Syntax
{
    public class DefaultSwitchLabelSyntax : SwitchLabelSyntax
    {
        private readonly SyntaxToken _colonToken;

        public readonly SyntaxToken DefaultKeyword;

        public override SyntaxToken Keyword => DefaultKeyword;
        public override SyntaxToken ColonToken => _colonToken;

        public DefaultSwitchLabelSyntax(SyntaxToken defaultKeyword, SyntaxToken colonToken)
            : base(SyntaxKind.DefaultSwitchLabel)
        {
            RegisterChildNode(out DefaultKeyword, defaultKeyword);
            RegisterChildNode(out _colonToken, colonToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitDefaultSwitchLabel(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitDefaultSwitchLabel(this);
        }
    }
}