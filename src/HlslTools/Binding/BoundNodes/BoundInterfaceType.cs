using System.Collections.Immutable;
using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundInterfaceType : BoundType
    {
        public InterfaceSymbol InterfaceSymbol { get; }
        public ImmutableArray<BoundFunction> Methods { get; }

        public BoundInterfaceType(InterfaceSymbol interfaceSymbol, ImmutableArray<BoundFunction> methods)
            : base(BoundNodeKind.InterfaceType)
        {
            InterfaceSymbol = interfaceSymbol;
            Methods = methods;
        }
    }
}