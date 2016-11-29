using ShaderTools.Hlsl.Symbols;

namespace ShaderTools.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundVariableExpression : BoundExpression
    {
        public BoundVariableExpression(VariableSymbol variableSymbol)
            : base(BoundNodeKind.VariableExpression)
        {
            Symbol = variableSymbol;
            Type = variableSymbol?.ValueType;
        }

        public override TypeSymbol Type { get; }
        public VariableSymbol Symbol { get; }
    }
}