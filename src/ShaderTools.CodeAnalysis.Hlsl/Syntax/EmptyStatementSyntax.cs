using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class EmptyStatementSyntax : StatementSyntax
    {
        public readonly SyntaxToken SemicolonToken;

        public EmptyStatementSyntax(List<AttributeSyntax> attributes, SyntaxToken semicolonToken)
            : base(SyntaxKind.EmptyStatement, attributes)
        {
            RegisterChildNode(out SemicolonToken, semicolonToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitEmptyStatement(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitEmptyStatement(this);
        }
    }
}