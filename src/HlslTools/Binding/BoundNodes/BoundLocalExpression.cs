using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundLocalExpression : BoundExpression
    {
        public BoundLocalExpression(SyntaxNode syntax, LocalSymbol symbol)
            : base(BoundNodeKind.LocalExpression, syntax)
        {
            Type = symbol.ValueType;
            Symbol = symbol;
        }

        public override TypeSymbol Type { get; }
        public LocalSymbol Symbol { get; }
    }
}