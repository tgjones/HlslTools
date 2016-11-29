using ShaderTools.Hlsl.Symbols;

namespace ShaderTools.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundFunctionName : BoundExpression
    {
        public BoundFunctionName(FunctionSymbol symbol)
            : base(BoundNodeKind.FunctionName)
        {
            Symbol = symbol;
            Type = null;
        }

        public override TypeSymbol Type { get; }
        public FunctionSymbol Symbol { get; }
    }
}