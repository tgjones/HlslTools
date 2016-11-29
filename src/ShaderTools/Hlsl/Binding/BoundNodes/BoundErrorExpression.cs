using ShaderTools.Hlsl.Symbols;

namespace ShaderTools.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundErrorExpression : BoundExpression
    {
        public BoundErrorExpression()
            : base(BoundNodeKind.ErrorExpression)
        {
            Type = TypeFacts.Unknown;
        }

        public override TypeSymbol Type { get; }
    }
}