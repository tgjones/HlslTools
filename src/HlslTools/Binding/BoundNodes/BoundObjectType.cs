using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundObjectType : BoundType
    {
        public IntrinsicObjectTypeSymbol ObjectSymbol { get; }

        public BoundObjectType(IntrinsicObjectTypeSymbol objectSymbol)
            : base(BoundNodeKind.IntrinsicObjectType, objectSymbol)
        {
            ObjectSymbol = objectSymbol;
        }
    }
}