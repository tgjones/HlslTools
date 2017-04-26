using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundStructType : BoundType
    {
        public StructSymbol StructSymbol { get; }
        public ImmutableArray<BoundNode> Members { get; }

        public BoundStructType(StructSymbol structSymbol, ImmutableArray<BoundNode> members)
            : base(BoundNodeKind.StructType, structSymbol)
        {
            StructSymbol = structSymbol;
            Members = members;
        }
    }
}