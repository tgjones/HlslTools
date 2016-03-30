using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundUnknownType : BoundType
    {
        public BoundUnknownType()
            : base(BoundNodeKind.UnknownType, TypeFacts.Unknown)
        {
        }
    }
}