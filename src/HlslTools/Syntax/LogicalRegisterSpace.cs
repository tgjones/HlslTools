namespace HlslTools.Syntax
{
    public class LogicalRegisterSpace : SyntaxNode
    {
        public readonly SyntaxToken CommaToken;
        public readonly SyntaxToken SpaceToken;

        public LogicalRegisterSpace(SyntaxToken commaToken, SyntaxToken spaceToken)
            : base(SyntaxKind.LogicalRegisterSpace)
        {
            RegisterChildNode(out CommaToken, commaToken);
            RegisterChildNode(out SpaceToken, spaceToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitLogicalRegisterSpace(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitLogicalRegisterSpace(this);
        }
    }
}