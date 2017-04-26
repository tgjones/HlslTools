namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public abstract class DeclarationNameSyntax : SyntaxNode
    {
        protected DeclarationNameSyntax(SyntaxKind kind)
            : base(kind)
        {

        }

        public abstract IdentifierDeclarationNameSyntax GetUnqualifiedName();
    }
}