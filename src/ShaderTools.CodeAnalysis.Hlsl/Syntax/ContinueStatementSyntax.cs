using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class ContinueStatementSyntax : StatementSyntax
    {
        public readonly SyntaxToken ContinueKeyword;
        public readonly SyntaxToken SemicolonToken;

        public ContinueStatementSyntax(List<AttributeSyntax> attributes, SyntaxToken continueKeyword, SyntaxToken semicolonToken)
            : base(SyntaxKind.ContinueStatement, attributes)
        {
            RegisterChildNode(out ContinueKeyword, continueKeyword);
            RegisterChildNode(out SemicolonToken, semicolonToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitContinueStatement(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitContinueStatement(this);
        }
    }
}