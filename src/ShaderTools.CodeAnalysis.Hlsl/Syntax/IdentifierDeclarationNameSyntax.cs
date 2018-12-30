namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public sealed partial class IdentifierDeclarationNameSyntax : DeclarationNameSyntax
    {
        public override IdentifierDeclarationNameSyntax GetUnqualifiedName()
        {
            return this;
        }
    }
}