using System.Collections.Immutable;
using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundArrayInitializerExpression : BoundExpression
    {
        public BoundArrayInitializerExpression(ImmutableArray<BoundExpression> elements)
            : base(BoundNodeKind.ArrayInitializerExpression)
        {
            // TODO: type should be the common parent type.
            Type = (elements.Length > 0) ? elements[0].Type : TypeFacts.Unknown;
            Elements = elements;
        }

        public override TypeSymbol Type { get; }

        public ImmutableArray<BoundExpression> Elements { get; }
    }
}