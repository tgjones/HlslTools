using System.Collections.Immutable;
using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundStructType : BoundNode
    {
        public StructSymbol StructSymbol { get; }
        public ImmutableArray<BoundStatement> Fields { get; }

        public BoundStructType(StructTypeSyntax syntax, StructSymbol structSymbol, ImmutableArray<BoundStatement> fields)
            : base(BoundNodeKind.StructType, syntax)
        {
            StructSymbol = structSymbol;
            Fields = fields;
        }
    }
}