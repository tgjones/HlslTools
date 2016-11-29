using System.Collections.Generic;

namespace ShaderTools.Hlsl.Syntax
{
    public class BlockSyntax : StatementSyntax
    {
        public readonly SyntaxToken OpenBraceToken;
        public readonly List<StatementSyntax> Statements;
        public readonly SyntaxToken CloseBraceToken;

        public BlockSyntax(List<AttributeSyntax> attributes, SyntaxToken openBraceToken, List<StatementSyntax> statements, SyntaxToken closeBraceToken)
            : base(SyntaxKind.Block, attributes)
        {
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNodes(out Statements, statements);
            RegisterChildNode(out CloseBraceToken, closeBraceToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitBlock(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitBlock(this);
        }
    }
}