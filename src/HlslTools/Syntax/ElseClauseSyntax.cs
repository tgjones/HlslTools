namespace HlslTools.Syntax
{
    public class ElseClauseSyntax : SyntaxNode
    {
        public readonly SyntaxToken ElseKeyword;
        public readonly StatementSyntax Statement;

        public ElseClauseSyntax(SyntaxToken elseKeyword, StatementSyntax statement)
            : base(SyntaxKind.ElseClause)
        {
            RegisterChildNode(out ElseKeyword, elseKeyword);
            RegisterChildNode(out Statement, statement);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitElseClause(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitElseClause(this);
        }
    }
}