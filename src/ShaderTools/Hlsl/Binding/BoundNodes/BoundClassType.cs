using System.Collections.Immutable;
using ShaderTools.Hlsl.Symbols;

namespace ShaderTools.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundClassType : BoundType
    {
        public ClassSymbol ClassSymbol { get; }
        public ImmutableArray<BoundNode> Members { get; }

        public BoundClassType(ClassSymbol classSymbol, ImmutableArray<BoundNode> members)
            : base(BoundNodeKind.ClassType, classSymbol)
        {
            ClassSymbol = classSymbol;
            Members = members;
        }
    }
}