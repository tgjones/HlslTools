namespace ShaderTools.Hlsl.Syntax
{
    public class CaseSwitchLabelSyntax : SwitchLabelSyntax
    {
        private readonly SyntaxToken _colonToken;

        public readonly SyntaxToken CaseKeyword;
        public readonly ExpressionSyntax Value;

        public override SyntaxToken Keyword => CaseKeyword;
        public override SyntaxToken ColonToken => _colonToken;

        public CaseSwitchLabelSyntax(SyntaxToken caseKeyword, ExpressionSyntax value, SyntaxToken colonToken)
            : base(SyntaxKind.CaseSwitchLabel)
        {
            RegisterChildNode(out CaseKeyword, caseKeyword);
            RegisterChildNode(out Value, value);
            RegisterChildNode(out _colonToken, colonToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCaseSwitchLabel(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCaseSwitchLabel(this);
        }
    }
}