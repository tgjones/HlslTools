namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public abstract class VariableDeclaratorQualifierSyntax : SyntaxNode
    {
        protected VariableDeclaratorQualifierSyntax(SyntaxKind kind)
            : base(kind)
        {
        }
    }
}