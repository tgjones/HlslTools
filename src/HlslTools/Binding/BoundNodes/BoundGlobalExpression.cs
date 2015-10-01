using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundGlobalExpression : BoundExpression
    {
        public BoundGlobalExpression(SyntaxNode syntax, GlobalSymbol symbol)
            : base(BoundNodeKind.GlobalExpression, syntax)
        {
            Type = symbol.ValueType;
            Symbol = symbol;
        }

        public override TypeSymbol Type { get; }
        public GlobalSymbol Symbol { get; }
    }
}