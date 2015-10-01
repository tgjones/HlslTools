using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundUnaryExpression : BoundExpression
    {
        public BoundUnaryExpression(SyntaxNode syntax, BoundExpression expression, UnaryOperatorKind operatorKind, TypeSymbol expressionType)
            : base(BoundNodeKind.UnaryExpression, syntax)
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