using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class DiscardStatementSyntax : StatementSyntax
    {
        public readonly SyntaxToken DiscardKeyword;
        public readonly SyntaxToken SemicolonToken;

        public DiscardStatementSyntax(List<AttributeSyntax> attributes, SyntaxToken discardKeyword, SyntaxToken semicolonToken)
            : base(SyntaxKind.DiscardStatement, attributes)
        {
            RegisterChildNode(out DiscardKeyword, discardKeyword);
            RegisterChildNode(out SemicolonToken, semicolonToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitDiscardStatement(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitDiscardStatement(this);
        }
    }
}