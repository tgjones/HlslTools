using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundName : BoundType
    {
        public BoundName(TypeSymbol typeSymbol)
            : base(BoundNodeKind.Name, typeSymbol)
        {
        }
    }
}