namespace HlslTools.Syntax
{
    public sealed class IncompleteDeclarationSyntax : SyntaxNode
    {
        public IncompleteDeclarationSyntax(SourceRange sourceRange)
            : base(SyntaxKind.IncompleteMember)
        {
            SourceRange = sourceRange;
            FullSourceRange = sourceRange;
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            throw new System.NotImplementedException();
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            throw new System.NotImplementedException();
        }
    }
}