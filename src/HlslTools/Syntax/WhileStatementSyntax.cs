using System.Collections.Generic;

namespace HlslTools.Syntax
{
    public class WhileStatementSyntax : StatementSyntax
    {
        public readonly SyntaxToken WhileKeyword;
        public readonly SyntaxToken OpenParenToken;
        public readonly ExpressionSyntax Condition;
        public readonly SyntaxToken CloseParenToken;
        public readonly StatementSyntax Statement;

        public WhileStatementSyntax(List<AttributeSyntax> attributes, SyntaxToken whileKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement)
            : base(SyntaxKind.WhileStatement, attributes)
        {
            RegisterChildNode(out WhileKeyword, whileKeyword);
            RegisterChildNode(out OpenParenToken, openParenToken);
            RegisterChildNode(out Condition, condition);
            RegisterChildNode(out CloseParenToken, closeParenToken);
            RegisterChildNode(out Statement, statement);
        }


        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitWhileStatement(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitWhileStatement(this);
        }
    }
}