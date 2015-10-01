namespace HlslTools.Syntax
{
    public sealed class IdentifierDeclarationNameSyntax : DeclarationNameSyntax
    {
        public readonly SyntaxToken Name;

        public IdentifierDeclarationNameSyntax(SyntaxToken name)
            : base(SyntaxKind.IdentifierDeclarationName)
        {
            RegisterChildNode(out Name, name);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitIdentifierDeclarationName(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitIdentifierDeclarationName(this);
        }

        public override IdentifierDeclarationNameSyntax GetUnqualifiedName()
        {
            return this;
        }
    }
}