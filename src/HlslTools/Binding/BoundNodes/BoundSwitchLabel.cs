using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundSwitchLabel : BoundNode
    {
        public BoundExpression Expression { get; set; }

        public BoundSwitchLabel(SwitchLabelSyntax syntax, BoundExpression expression)
            : base(BoundNodeKind.SwitchLabel, syntax)
        {
            Expression = expression;
        }
    }
}