namespace HlslTools.Syntax
{
    public class PackOffsetComponentPart : SyntaxNode
    {
        public readonly SyntaxToken DotToken;
        public readonly SyntaxToken Component;

        public PackOffsetComponentPart(SyntaxToken dotToken, SyntaxToken component)
            : base(SyntaxKind.PackOffsetComponentPart)
        {
            RegisterChildNode(out DotToken, dotToken);
            RegisterChildNode(out Component, component);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitPackOffsetComponentPart(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitPackOffsetComponentPart(this);
        }
    }
}