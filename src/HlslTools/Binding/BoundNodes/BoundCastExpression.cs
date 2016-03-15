using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundCastExpression : BoundExpression
    {
        public override TypeSymbol Type { get; }
        public BoundExpression Expression { get; }

        public BoundCastExpression(TypeSymbol targetType, BoundExpression expression)
            : base(BoundNodeKind.CastExpression)
        {
            Type = targetType;
            Expression = expression;
        }
    }
}