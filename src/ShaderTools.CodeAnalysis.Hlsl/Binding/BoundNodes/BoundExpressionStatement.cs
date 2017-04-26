namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundExpressionStatement : BoundStatement
    {
        public BoundExpressionStatement(BoundExpression expression)
            : base(BoundNodeKind.ExpressionStatement)
        {
            Expression = expression;
        }

        public BoundExpression Expression { get; set; }
    }
}