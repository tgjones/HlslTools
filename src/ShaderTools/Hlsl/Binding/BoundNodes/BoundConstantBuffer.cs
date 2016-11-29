using System.Collections.Immutable;
using ShaderTools.Hlsl.Symbols;

namespace ShaderTools.Hlsl.Binding.BoundNodes
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