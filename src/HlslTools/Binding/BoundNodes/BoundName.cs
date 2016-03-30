using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundName : BoundType
    {
        public Symbol Symbol { get; }

        public BoundName(Symbol symbol)
            : base(BoundNodeKind.Name, symbol as TypeSymbol ?? TypeFacts.Missing)
        {
            Symbol = symbol;
        }
    }
}