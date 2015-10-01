using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundLiteralExpression : BoundExpression
    {
        public BoundLiteralExpression(LiteralExpressionSyntax syntax, TypeSymbol type)
            : base(BoundNodeKind.LiteralExpression, syntax)
        {
            Type = type;
            Value = syntax.Token.Value;
        }

        public override TypeSymbol Type { get; }
        public object Value { get; }
    }
}