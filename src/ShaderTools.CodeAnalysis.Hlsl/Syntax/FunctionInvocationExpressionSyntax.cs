namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class FunctionInvocationExpressionSyntax : InvocationExpressionSyntax
    {
        public readonly NameSyntax Name;

        public FunctionInvocationExpressionSyntax(NameSyntax name, ArgumentListSyntax argumentList)
            : base(SyntaxKind.FunctionInvocationExpression, argumentList)
        {
            RegisterChildNode(out Name, name);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitFunctionInvocationExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitFunctionInvocationExpression(this);
        }
    }
}