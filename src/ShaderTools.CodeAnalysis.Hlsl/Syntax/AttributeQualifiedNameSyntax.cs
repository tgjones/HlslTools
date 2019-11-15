namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    partial class AttributeQualifiedNameSyntax
    {
        public override IdentifierNameSyntax GetUnqualifiedName()
        {
            return Right;
        }
    }
}
