using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundFunction : BoundNode
    {
        public FunctionSymbol FunctionSymbol { get; }

        public BoundFunction(FunctionSymbol functionSymbol)
            : base(BoundNodeKind.Function)
        {
            FunctionSymbol = functionSymbol;
        }
    }
}