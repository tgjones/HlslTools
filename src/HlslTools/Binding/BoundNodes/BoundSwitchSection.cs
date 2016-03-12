using System.Collections.Immutable;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundSwitchSection : BoundNode
    {
        public ImmutableArray<BoundSwitchLabel> Labels { get; set; }
        public ImmutableArray<BoundStatement> Statements { get; set; }

        public BoundSwitchSection(ImmutableArray<BoundSwitchLabel> labels, ImmutableArray<BoundStatement> statements)
            : base(BoundNodeKind.SwitchSection)
        {
            Labels = labels;
            Statements = statements;
        }
    }
}