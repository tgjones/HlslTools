using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundClassType : BoundNode
    {
        public ClassSymbol ClassSymbol { get; }

        public BoundClassType(ClassSymbol classSymbol)
            : base(BoundNodeKind.ClassType)
        {
            ClassSymbol = classSymbol;
        }
    }
}