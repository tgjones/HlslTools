namespace HlslTools.Syntax
{
    public sealed class QualifiedNameSyntax : NameSyntax
    {
        public readonly NameSyntax Left;
        public readonly SyntaxToken ColonColonToken;
        public readonly IdentifierNameSyntax Right;

        public QualifiedNameSyntax(NameSyntax left, SyntaxToken colonColonToken, IdentifierNameSyntax right)
            : base(SyntaxKind.QualifiedName)
        {
            RegisterChildNode(out Left, left);
            RegisterChildNode(out ColonColonToken, colonColonToken);
            RegisterChildNode(out Right, right);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitQualifiedName(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitQualifiedName(this);
        }
    }
}