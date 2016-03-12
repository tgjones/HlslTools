using System.Collections.Immutable;
using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundStructType : BoundNode
    {
        public StructSymbol StructSymbol { get; }
        public ImmutableArray<BoundStatement> Fields { get; }

        public BoundStructType(StructSymbol structSymbol, ImmutableArray<BoundStatement> fields)
            : base(BoundNodeKind.StructType)
        {
            StructSymbol = structSymbol;
            Fields = fields;
        }
    }
}