using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundInterfaceType : BoundNode
    {
        public InterfaceSymbol InterfaceSymbol { get; }

        public BoundInterfaceType(InterfaceSymbol interfaceSymbol)
            : base(BoundNodeKind.ClassType)
        {
            InterfaceSymbol = interfaceSymbol;
        }
    }
}