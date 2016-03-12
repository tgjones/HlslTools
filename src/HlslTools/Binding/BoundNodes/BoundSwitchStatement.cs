using System.Collections.Immutable;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundSwitchStatement : BoundStatement
    {
        public BoundExpression Expression { get; set; }
        public ImmutableArray<BoundSwitchSection> Sections { get; set; }

        public BoundSwitchStatement(BoundExpression expression, ImmutableArray<BoundSwitchSection> sections)
            : base(BoundNodeKind.SwitchStatement)
        {
            Expression = expression;
            Sections = sections;
        }
    }
}