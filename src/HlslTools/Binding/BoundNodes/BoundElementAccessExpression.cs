using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundElementAccessExpression : BoundExpression
    {
        public override TypeSymbol Type { get; }

        public BoundExpression Expression { get; }
        public BoundExpression Index { get; }

        public BoundElementAccessExpression(ElementAccessExpressionSyntax syntax, BoundExpression expression, BoundExpression index, IndexerSymbol indexer)
            : base(BoundNodeKind.ElementAccessExpression, syntax)
        {
            Expression = expression;
            Index = index;
            Type = indexer.ValueType;
        }
    }
}