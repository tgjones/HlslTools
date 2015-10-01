namespace HlslTools.Syntax
{
    public class ParameterListSyntax : SyntaxNode
    {
        public readonly SyntaxToken OpenParenToken;
        public readonly SeparatedSyntaxList<ParameterSyntax> Parameters;
        public readonly SyntaxToken CloseParenToken;

        public ParameterListSyntax(SyntaxToken openParenToken, SeparatedSyntaxList<ParameterSyntax> parameters, SyntaxToken closeParenToken)
            : base(SyntaxKind.ArgumentList)
        {
            RegisterChildNode(out OpenParenToken, openParenToken);
            RegisterChildNodes(out Parameters, parameters);
            RegisterChildNode(out CloseParenToken, closeParenToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitParameterList(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitParameterList(this);
        }
    }
}