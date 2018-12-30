namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public abstract partial class DeclarationNameSyntax : SyntaxNode
    {
        public abstract IdentifierDeclarationNameSyntax GetUnqualifiedName();
    }
}