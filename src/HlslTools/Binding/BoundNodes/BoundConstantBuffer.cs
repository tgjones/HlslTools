using System.Collections.Immutable;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundConstantBuffer : BoundNode
    {
        public ImmutableArray<BoundNode> Variables { get; }

        public BoundConstantBuffer(ImmutableArray<BoundNode> variables)
            : base(BoundNodeKind.ConstantBuffer)
        {
            Variables = variables;
        }
    }
}