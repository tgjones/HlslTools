using System.Collections.Generic;

namespace ShaderTools.Hlsl.Syntax
{
    public class BreakStatementSyntax : StatementSyntax
    {
        public readonly SyntaxToken BreakKeyword;
        public readonly SyntaxToken SemicolonToken;

        public BreakStatementSyntax(List<AttributeSyntax> attributes, SyntaxToken breakKeyword, SyntaxToken semicolonToken)
            : base(SyntaxKind.BreakStatement, attributes)
        {
            RegisterChildNode(out BreakKeyword, breakKeyword);
            RegisterChildNode(out SemicolonToken, semicolonToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitBreakStatement(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitBreakStatement(this);
        }
    }
}