using System.Collections.Generic;

namespace HlslTools.Syntax
{
    public class DoStatementSyntax : StatementSyntax
    {
        public readonly SyntaxToken DoKeyword;
        public readonly StatementSyntax Statement;
        public readonly SyntaxToken WhileKeyword;
        public readonly SyntaxToken OpenParenToken;
        public readonly ExpressionSyntax Condition;
        public readonly SyntaxToken CloseParenToken;
        public readonly SyntaxToken SemicolonToken;

        public DoStatementSyntax(List<AttributeSyntax> attributes, SyntaxToken doKeyword, StatementSyntax statement, SyntaxToken whileKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, SyntaxToken semicolonToken)
            : base(SyntaxKind.DoStatement, attributes)
        {
            RegisterChildNode(out DoKeyword, doKeyword);
            RegisterChildNode(out Statement, statement);
            RegisterChildNode(out WhileKeyword, whileKeyword);
            RegisterChildNode(out OpenParenToken, openParenToken);
            RegisterChildNode(out Condition, condition);
            RegisterChildNode(out CloseParenToken, closeParenToken);
            RegisterChildNode(out SemicolonToken, semicolonToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitDoStatement(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitDoStatement(this);
        }
    }
}