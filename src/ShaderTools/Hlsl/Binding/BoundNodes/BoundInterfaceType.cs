using System.Collections.Immutable;
using ShaderTools.Hlsl.Symbols;

namespace ShaderTools.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundInterfaceType : BoundType
    {
        public InterfaceSymbol InterfaceSymbol { get; }
        public ImmutableArray<BoundFunction> Methods { get; }

        public BoundInterfaceType(InterfaceSymbol interfaceSymbol, ImmutableArray<BoundFunction> methods)
            : base(BoundNodeKind.InterfaceType, interfaceSymbol)
        {
            InterfaceSymbol = interfaceSymbol;
            Methods = methods;
        }
    }
}