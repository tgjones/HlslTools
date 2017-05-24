namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public abstract class InvocationExpressionSyntax : ExpressionSyntax
    {
        public readonly ArgumentListSyntax ArgumentList;

        protected InvocationExpressionSyntax(SyntaxKind kind, ArgumentListSyntax argumentList)
            : base(kind)
        {
            RegisterChildNode(out ArgumentList, argumentList);
        }
    }
}