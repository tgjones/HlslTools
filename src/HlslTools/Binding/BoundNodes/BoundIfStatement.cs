namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundIfStatement : BoundStatement
    {
        public BoundExpression Condition { get; }
        public BoundStatement Consequence { get; }
        public BoundStatement AlternativeOpt { get; }

        public BoundIfStatement(BoundExpression condition, BoundStatement consequence, BoundStatement alternativeOpt)
            : base(BoundNodeKind.IfStatement)
        {
            Condition = condition;
            Consequence = consequence;
            AlternativeOpt = alternativeOpt;
        }
    }
}