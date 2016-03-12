using System.Collections.Immutable;
using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundClassType : BoundNode
    {
        public ClassSymbol ClassSymbol { get; }
        public ImmutableArray<BoundNode> Members { get; }

        public BoundClassType(ClassSymbol classSymbol, ImmutableArray<BoundNode> members)
            : base(BoundNodeKind.ClassType)
        {
            ClassSymbol = classSymbol;
            Members = members;
        }
    }
}