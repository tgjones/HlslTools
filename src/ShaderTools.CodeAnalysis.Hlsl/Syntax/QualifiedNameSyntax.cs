namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public sealed partial class QualifiedNameSyntax : NameSyntax
    {
        public override IdentifierNameSyntax GetUnqualifiedName()
        {
            return Right;
        }
    }
}