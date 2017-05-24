namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class NumericConstructorInvocationExpressionSyntax : InvocationExpressionSyntax
    {
        public readonly NumericTypeSyntax Type;
        public readonly ArgumentListSyntax ArgumentList;

        public override ArgumentListSyntax ArgumentListSyntax => ArgumentList;

        public NumericConstructorInvocationExpressionSyntax(NumericTypeSyntax type, ArgumentListSyntax argumentList)
            : base(SyntaxKind.NumericConstructorInvocationExpression)
        {
            RegisterChildNode(out Type, type);
            RegisterChildNode(out ArgumentList, argumentList);
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