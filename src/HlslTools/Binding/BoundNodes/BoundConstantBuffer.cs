using System.Collections.Immutable;
using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundConstantBuffer : BoundNode
    {
        public ConstantBufferSymbol ConstantBufferSymbol { get; }
        public ImmutableArray<BoundMultipleVariableDeclarations> Variables { get; }

        public BoundConstantBuffer(ConstantBufferSymbol constantBufferSymbol, ImmutableArray<BoundMultipleVariableDeclarations> variables)
            : base(BoundNodeKind.ConstantBuffer)
        {
            ConstantBufferSymbol = constantBufferSymbol;
            Variables = variables;
        }
    }
}