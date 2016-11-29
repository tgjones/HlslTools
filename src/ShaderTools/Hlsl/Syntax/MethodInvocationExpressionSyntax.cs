namespace ShaderTools.Hlsl.Syntax
{
    public sealed class MethodInvocationExpressionSyntax : ExpressionSyntax
    {
        public readonly ExpressionSyntax Target;
        public readonly SyntaxToken DotToken;
        public readonly SyntaxToken Name;
        public readonly ArgumentListSyntax ArgumentList;

        public MethodInvocationExpressionSyntax(ExpressionSyntax target, SyntaxToken dot, SyntaxToken name, ArgumentListSyntax argumentList)
            : base(SyntaxKind.MethodInvocationExpression)
        {
            RegisterChildNode(out Target, target);
            RegisterChildNode(out DotToken, dot);
            RegisterChildNode(out Name, name);
            RegisterChildNode(out ArgumentList, argumentList);
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