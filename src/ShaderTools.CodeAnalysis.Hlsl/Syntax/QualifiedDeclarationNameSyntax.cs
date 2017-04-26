namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public sealed class QualifiedDeclarationNameSyntax : DeclarationNameSyntax
    {
        public readonly DeclarationNameSyntax Left;
        public readonly SyntaxToken ColonColonToken;
        public readonly IdentifierDeclarationNameSyntax Right;

        public QualifiedDeclarationNameSyntax(DeclarationNameSyntax left, SyntaxToken colonColonToken, IdentifierDeclarationNameSyntax right)
            : base(SyntaxKind.QualifiedDeclarationName)
        {
            RegisterChildNode(out Left, left);
            RegisterChildNode(out ColonColonToken, colonColonToken);
            RegisterChildNode(out Right, right);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitQualifiedDeclarationName(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitQualifiedDeclarationName(this);
        }

        public override IdentifierDeclarationNameSyntax GetUnqualifiedName()
        {
            return Right;
        }
    }
}