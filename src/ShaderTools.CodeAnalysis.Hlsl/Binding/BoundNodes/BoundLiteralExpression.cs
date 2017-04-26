using ShaderTools.CodeAnalysis.Hlsl.Symbols;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
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