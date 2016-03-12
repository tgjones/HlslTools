using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundUnaryExpression : BoundExpression
    {
        public BoundUnaryExpression(BoundExpression expression, UnaryOperatorKind operatorKind, TypeSymbol expressionType)
            : base(BoundNodeKind.UnaryExpression)
        {
            Expression = expression;
            OperatorKind = operatorKind;
            Type = expressionType;
        }

        public BoundExpression Expression { get; set; }
        public UnaryOperatorKind OperatorKind { get; set; }
        public override TypeSymbol Type { get; }
    }
}