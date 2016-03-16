using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundErrorExpression : BoundExpression
    {
        public BoundErrorExpression()
            : base(BoundNodeKind.Error)
        {
            Type = TypeFacts.Unknown;
        }

        public override TypeSymbol Type { get; }
    }
}