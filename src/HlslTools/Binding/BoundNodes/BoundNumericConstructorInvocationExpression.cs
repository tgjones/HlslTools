using System.Collections.Immutable;
using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundNumericConstructorInvocationExpression : BoundExpression
    {
        public BoundNumericConstructorInvocationExpression(TypeSymbol type, ImmutableArray<BoundExpression> arguments)
            : base(BoundNodeKind.NumericConstructorInvocationExpression)
        {
            Type = type;
            Arguments = arguments;
        }

        public override TypeSymbol Type { get; }

        public ImmutableArray<BoundExpression> Arguments { get; }
    }
}