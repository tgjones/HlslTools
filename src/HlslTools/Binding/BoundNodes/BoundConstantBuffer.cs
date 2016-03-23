using System.Collections.Immutable;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundConstantBuffer : BoundNode
    {
        public ImmutableArray<BoundMultipleVariableDeclarations> Variables { get; }

        public BoundConstantBuffer(ImmutableArray<BoundMultipleVariableDeclarations> variables)
            : base(BoundNodeKind.ConstantBuffer)
        {
            Variables = variables;
        }
    }
}