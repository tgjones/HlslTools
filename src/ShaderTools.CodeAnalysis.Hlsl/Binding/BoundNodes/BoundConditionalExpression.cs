using ShaderTools.CodeAnalysis.Hlsl.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundConditionalExpression : BoundExpression
    {
        public override TypeSymbol Type { get; }

        public BoundExpression Condition { get; }
        public BoundExpression Consequence { get; }
        public BoundExpression Alternative { get; }

        public BoundConditionalExpression(BoundExpression condition, BoundExpression consequence, BoundExpression alternative)
            : base(BoundNodeKind.ConditionalExpression)
        {
            Condition = condition;
            Consequence = consequence;
            Alternative = alternative;
            Type = consequence.Type;
        }
    }
}