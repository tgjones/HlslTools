using System.Collections.Immutable;
using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundStructType : BoundType
    {
        public StructSymbol StructSymbol { get; }
        public ImmutableArray<BoundMultipleVariableDeclarations> Variables { get; }

        public BoundStructType(StructSymbol structSymbol, ImmutableArray<BoundMultipleVariableDeclarations> variables)
            : base(BoundNodeKind.StructType, structSymbol)
        {
            StructSymbol = structSymbol;
            Variables = variables;
        }
    }
}