namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public abstract class InvocationExpressionSyntax : ExpressionSyntax
    {
        public abstract ArgumentListSyntax ArgumentListSyntax { get; }

        protected InvocationExpressionSyntax(SyntaxKind kind)
            : base(kind)
        {
            
        }
    }
}