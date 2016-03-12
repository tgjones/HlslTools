namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundWhileStatement : BoundLoopStatement
    {
        public BoundWhileStatement(BoundExpression condition, BoundStatement body)
            : base(BoundNodeKind.WhileStatement)
        {
            Condition = condition;
            Body = body;
        }

        public BoundExpression Condition { get; }
        public BoundStatement Body { get; }
    }
}