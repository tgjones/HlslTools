using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundStructType : BoundNode
    {
        public StructSymbol StructSymbol { get; }

        public BoundStructType(StructSymbol structSymbol)
            : base(BoundNodeKind.StructType)
        {
            StructSymbol = structSymbol;
        }
    }
}