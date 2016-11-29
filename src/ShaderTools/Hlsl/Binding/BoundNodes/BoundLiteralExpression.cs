using ShaderTools.Hlsl.Symbols;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundLiteralExpression : BoundExpression
    {
        public BoundLiteralExpression(LiteralExpressionSyntax syntax, TypeSymbol type)
            : base(BoundNodeKind.LiteralExpression)
        {
            Type = type;
            Value = syntax.Token.Value;
        }

        public override TypeSymbol Type { get; }
        public object Value { get; }
    }
}