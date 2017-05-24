namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class NumericConstructorInvocationExpressionSyntax : InvocationExpressionSyntax
    {
        public readonly NumericTypeSyntax Type;

        public NumericConstructorInvocationExpressionSyntax(NumericTypeSyntax type, ArgumentListSyntax argumentList)
            : base(SyntaxKind.NumericConstructorInvocationExpression, argumentList)
        {
            RegisterChildNode(out Type, type);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitNumericConstructorInvocation(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitNumericConstructorInvocation(this);
        }
    }
}