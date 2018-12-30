namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public sealed partial class QualifiedDeclarationNameSyntax : DeclarationNameSyntax
    {
        public override IdentifierDeclarationNameSyntax GetUnqualifiedName()
        {
            return Right;
        }
    }
}