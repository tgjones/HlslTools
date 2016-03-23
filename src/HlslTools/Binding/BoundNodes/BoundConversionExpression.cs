using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundConversionExpression : BoundExpression
    {
        public BoundConversionExpression(BoundExpression expression, TypeSymbol type)
            : base(BoundNodeKind.ConversionExpression)
        {
            Expression = expression;
            Type = type;
        }

        public override TypeSymbol Type { get; }

        public BoundExpression Expression { get; }

        public BoundConversionExpression Update(BoundExpression expression, TypeSymbol type)
        {
            if (expression == Expression && type == Type)
                return this;

            return new BoundConversionExpression(expression, type);
        }
    }
}