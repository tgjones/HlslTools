using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class ReturnStatementSyntax : StatementSyntax
    {
        public readonly SyntaxToken ReturnKeyword;
        public readonly ExpressionSyntax Expression;
        public readonly SyntaxToken SemicolonToken;

        public ReturnStatementSyntax(List<AttributeSyntax> attributes, SyntaxToken returnKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken)
            : base(SyntaxKind.ReturnStatement, attributes)
        {
            RegisterChildNode(out ReturnKeyword, returnKeyword);
            RegisterChildNode(out Expression, expression);
            RegisterChildNode(out SemicolonToken, semicolonToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitReturnStatement(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitReturnStatement(this);
        }
    }
}