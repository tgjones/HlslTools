using System.Collections.Generic;

namespace ShaderTools.Hlsl.Syntax
{
    public class SwitchStatementSyntax : StatementSyntax
    {
        public readonly SyntaxToken SwitchKeyword;
        public readonly SyntaxToken OpenParenToken;
        public readonly ExpressionSyntax Expression;
        public readonly SyntaxToken CloseParenToken;
        public readonly SyntaxToken OpenBraceToken;
        public readonly List<SwitchSectionSyntax> Sections;
        public readonly SyntaxToken CloseBraceToken;

        public SwitchStatementSyntax(List<AttributeSyntax> attributes, SyntaxToken switchKeyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken, SyntaxToken openBraceToken, List<SwitchSectionSyntax> sections, SyntaxToken closeBraceToken)
            : base(SyntaxKind.SwitchStatement, attributes)
        {
            RegisterChildNode(out SwitchKeyword, switchKeyword);
            RegisterChildNode(out OpenParenToken, openParenToken);
            RegisterChildNode(out Expression, expression);
            RegisterChildNode(out CloseParenToken, closeParenToken);
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNodes(out Sections, sections);
            RegisterChildNode(out CloseBraceToken, closeBraceToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitSwitchStatement(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitSwitchStatement(this);
        }
    }
}