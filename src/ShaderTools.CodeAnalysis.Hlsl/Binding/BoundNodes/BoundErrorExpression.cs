using ShaderTools.CodeAnalysis.Hlsl.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
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