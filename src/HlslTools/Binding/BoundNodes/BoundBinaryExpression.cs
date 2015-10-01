using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundBinaryExpression : BoundExpression
    {
        public override TypeSymbol Type { get; }

        public BinaryOperatorKind OperatorKind { get; }
        public BoundExpression Left { get; }
        public BoundExpression Right { get; }

        public BoundBinaryExpression(BinaryExpressionSyntax syntax, BinaryOperatorKind operatorKind, BoundExpression left, BoundExpression right, TypeSymbol type)
            : base(BoundNodeKind.BinaryExpression, syntax)
        {
            OperatorKind = operatorKind;
            Left = left;
            Right = right;
            Type = type;
        }
    }
}