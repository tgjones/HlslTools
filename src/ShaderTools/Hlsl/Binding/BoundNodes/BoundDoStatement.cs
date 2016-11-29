namespace ShaderTools.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundDoStatement : BoundLoopStatement
    {
        public BoundDoStatement(BoundExpression condition, BoundStatement body)
            : base(BoundNodeKind.DoStatement)
        {
            Condition = condition;
            Body = body;
        }

        public BoundExpression Condition { get; }
        public BoundStatement Body { get; }
    }
}