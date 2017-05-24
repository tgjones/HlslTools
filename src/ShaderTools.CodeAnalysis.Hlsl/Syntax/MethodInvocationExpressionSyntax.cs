namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public sealed class MethodInvocationExpressionSyntax : InvocationExpressionSyntax
    {
        public readonly ExpressionSyntax Target;
        public readonly SyntaxToken DotToken;
        public readonly SyntaxToken Name;

        public MethodInvocationExpressionSyntax(ExpressionSyntax target, SyntaxToken dot, SyntaxToken name, ArgumentListSyntax argumentList)
            : base(SyntaxKind.MethodInvocationExpression, argumentList)
        {
            RegisterChildNode(out Target, target);
            RegisterChildNode(out DotToken, dot);
            RegisterChildNode(out Name, name);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitMethodInvocationExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitMethodInvocationExpression(this);
        }
    }
}