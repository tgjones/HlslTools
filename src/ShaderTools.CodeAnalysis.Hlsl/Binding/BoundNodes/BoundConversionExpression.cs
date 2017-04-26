using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundConversionExpression : BoundExpression
    {
        public BoundConversionExpression(BoundExpression expression, TypeSymbol type, Conversion conversion)
            : base(BoundNodeKind.ConversionExpression)
        {
            Expression = expression;
            Type = type;
            Conversion = conversion;
        }

        public override TypeSymbol Type { get; }
        public Conversion Conversion { get; }

        public BoundExpression Expression { get; }
    }
}