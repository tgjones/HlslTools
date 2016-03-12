using System.Collections.Immutable;
using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundInterfaceType : BoundNode
    {
        public InterfaceSymbol InterfaceSymbol { get; }
        public ImmutableArray<BoundNode> Members { get; }

        public BoundInterfaceType(InterfaceSymbol interfaceSymbol, ImmutableArray<BoundNode> members)
            : base(BoundNodeKind.ClassType)
        {
            InterfaceSymbol = interfaceSymbol;
            Members = members;
        }
    }
}