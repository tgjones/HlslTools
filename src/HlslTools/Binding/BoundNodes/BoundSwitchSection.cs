using System.Collections.Immutable;
using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundSwitchSection : BoundNode
    {
        public ImmutableArray<BoundSwitchLabel> Labels { get; set; }
        public ImmutableArray<BoundStatement> Statements { get; set; }

        public BoundSwitchSection(SwitchSectionSyntax syntax, ImmutableArray<BoundSwitchLabel> labels, ImmutableArray<BoundStatement> statements)
            : base(BoundNodeKind.SwitchSection, syntax)
        {
            Labels = labels;
            Statements = statements;
        }
    }
}