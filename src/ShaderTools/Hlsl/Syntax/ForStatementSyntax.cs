using System.Collections.Generic;

namespace ShaderTools.Hlsl.Syntax
{
    public class ForStatementSyntax : StatementSyntax
    {
        public readonly SyntaxToken ForKeyword;
        public readonly SyntaxToken OpenParenToken;
        public readonly VariableDeclarationSyntax Declaration;
        public readonly ExpressionSyntax Initializer;
        public readonly SyntaxToken FirstSemicolonToken;
        public readonly ExpressionSyntax Condition;
        public readonly SyntaxToken SecondSemicolonToken;
        public readonly ExpressionSyntax Incrementor;
        public readonly SyntaxToken CloseParenToken;
        public readonly StatementSyntax Statement;

        public ForStatementSyntax(List<AttributeSyntax> attributes, SyntaxToken forKeyword, SyntaxToken openParenToken, VariableDeclarationSyntax declaration, ExpressionSyntax initializer, SyntaxToken firstSemicolonToken, ExpressionSyntax condition, SyntaxToken secondSemicolonToken, ExpressionSyntax incrementor, SyntaxToken closeParenToken, StatementSyntax statement)
            : base(SyntaxKind.ForStatement, attributes)
        {
            RegisterChildNode(out ForKeyword, forKeyword);
            RegisterChildNode(out OpenParenToken, openParenToken);
            RegisterChildNode(out Declaration, declaration);
            RegisterChildNode(out Initializer, initializer);
            RegisterChildNode(out FirstSemicolonToken, firstSemicolonToken);
            RegisterChildNode(out Condition, condition);
            RegisterChildNode(out SecondSemicolonToken, secondSemicolonToken);
            RegisterChildNode(out Incrementor, incrementor);
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