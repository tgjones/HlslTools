using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundElementAccessExpression : BoundExpression
    {
        public override TypeSymbol Type { get; }

        public BoundExpression Expression { get; }
        public BoundExpression Index { get; }

        public BoundElementAccessExpression(BoundExpression expression, BoundExpression index, IndexerSymbol indexer)
            : base(BoundNodeKind.ElementAccessExpression)
        {
            Expression = expression;
            Index = index;
            Type = indexer.ValueType;
        }
    }
}