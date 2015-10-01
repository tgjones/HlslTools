using System.Collections.Generic;

namespace HlslTools.Syntax
{
    public class ForStatementSyntax : StatementSyntax
    {
        public readonly SyntaxToken ForKeyword;
        public readonly SyntaxToken OpenParenToken;
        public readonly VariableDeclarationSyntax Declaration;
        public readonly SeparatedSyntaxList<ExpressionSyntax> Initializers;
        public readonly SyntaxToken FirstSemicolonToken;
        public readonly ExpressionSyntax Condition;
        public readonly SyntaxToken SecondSemicolonToken;
        public readonly SeparatedSyntaxList<ExpressionSyntax> Incrementors;
        public readonly SyntaxToken CloseParenToken;
        public readonly StatementSyntax Statement;

        public ForStatementSyntax(List<AttributeSyntax> attributes, SyntaxToken forKeyword, SyntaxToken openParenToken, VariableDeclarationSyntax declaration, SeparatedSyntaxList<ExpressionSyntax> initializers, SyntaxToken firstSemicolonToken, ExpressionSyntax condition, SyntaxToken secondSemicolonToken, SeparatedSyntaxList<ExpressionSyntax> incrementors, SyntaxToken closeParenToken, StatementSyntax statement)
            : base(SyntaxKind.ForStatement, attributes)
        {
            RegisterChildNode(out ForKeyword, forKeyword);
            RegisterChildNode(out OpenParenToken, openParenToken);
            RegisterChildNode(out Declaration, declaration);
            RegisterChildNodes(out Initializers, initializers);
            RegisterChildNode(out FirstSemicolonToken, firstSemicolonToken);
            RegisterChildNode(out Condition, condition);
            RegisterChildNode(out SecondSemicolonToken, secondSemicolonToken);
            RegisterChildNodes(out Incrementors, incrementors);
            RegisterChildNode(out CloseParenToken, closeParenToken);
            RegisterChildNode(out Statement, statement);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitForStatement(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitForStatement(this);
        }
    }
}