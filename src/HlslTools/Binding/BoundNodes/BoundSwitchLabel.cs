namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundSwitchLabel : BoundNode
    {
        public BoundExpression Expression { get; set; }

        public BoundSwitchLabel(BoundExpression expression)
            : base(BoundNodeKind.SwitchLabel)
        {
            Expression = expression;
        }
    }
}