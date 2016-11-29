using System.Collections.Generic;

namespace ShaderTools.Hlsl.Syntax
{
    public class IfStatementSyntax : StatementSyntax
    {
        public readonly SyntaxToken IfKeyword;
        public readonly SyntaxToken OpenParenToken;
        public readonly ExpressionSyntax Condition;
        public readonly SyntaxToken CloseParenToken;
        public readonly StatementSyntax Statement;
        public readonly ElseClauseSyntax Else;

        public IfStatementSyntax(List<AttributeSyntax> attributes, SyntaxToken ifKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement, ElseClauseSyntax @else)
            : base(SyntaxKind.IfStatement, attributes)
        {
            RegisterChildNode(out IfKeyword, ifKeyword);
            RegisterChildNode(out OpenParenToken, openParenToken);
            RegisterChildNode(out Condition, condition);
            RegisterChildNode(out CloseParenToken, closeParenToken);
            RegisterChildNode(out Statement, statement);
            RegisterChildNode(out Else, @else);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitIfStatement(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitIfStatement(this);
        }
    }
}