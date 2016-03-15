using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundMethodName : BoundExpression
    {
        public BoundMethodName(MethodSymbol symbol)
            : base(BoundNodeKind.MethodName)
        {
            Symbol = symbol;
            Type = null;
        }

        public override TypeSymbol Type { get; }
        public MethodSymbol Symbol { get; }
    }
}