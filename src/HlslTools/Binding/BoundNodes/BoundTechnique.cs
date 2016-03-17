using System.Collections.Immutable;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundTechnique : BoundNode
    {
        public ImmutableArray<BoundPass> Passes { get; }

        public BoundTechnique(ImmutableArray<BoundPass> passes)
            : base(BoundNodeKind.Technique)
        {
            Passes = passes;
        }
    }
}