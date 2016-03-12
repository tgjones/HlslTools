using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundBinaryExpression : BoundExpression
    {
        public override TypeSymbol Type { get; }

        public BinaryOperatorKind OperatorKind { get; }
        public BoundExpression Left { get; }
        public BoundExpression Right { get; }

        public BoundBinaryExpression(BinaryOperatorKind operatorKind, BoundExpression left, BoundExpression right, TypeSymbol type)
            : base(BoundNodeKind.BinaryExpression)
        {
            OperatorKind = operatorKind;
            Left = left;
            Right = right;
            Type = type;
        }
    }
}