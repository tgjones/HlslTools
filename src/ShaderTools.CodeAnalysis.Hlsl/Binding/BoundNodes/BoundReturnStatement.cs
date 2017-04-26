namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundReturnStatement : BoundStatement
    {
        public BoundReturnStatement(BoundExpression expressionOpt)
            : base(BoundNodeKind.ReturnStatement)
        {
            ExpressionOpt = expressionOpt;
        }

        public BoundExpression ExpressionOpt { get; }
    }
}