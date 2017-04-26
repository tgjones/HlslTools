using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class ExpressionStatementSyntax : StatementSyntax
    {
        public readonly ExpressionSyntax Expression;
        public readonly SyntaxToken SemicolonToken;

        public ExpressionStatementSyntax(List<AttributeSyntax> attributes, ExpressionSyntax expression, SyntaxToken semicolonToken)
            : base(SyntaxKind.ExpressionStatement, attributes)
        {
            RegisterChildNode(out Expression, expression);
            RegisterChildNode(out SemicolonToken, semicolonToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitExpressionStatement(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitExpressionStatement(this);
        }
    }
}