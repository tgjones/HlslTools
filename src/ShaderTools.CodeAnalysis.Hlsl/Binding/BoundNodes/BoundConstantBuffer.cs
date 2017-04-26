using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
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