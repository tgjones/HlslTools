namespace HlslTools.Symbols
{
    public sealed class VariadicParameterSymbol : ParameterSymbol
    {
        public VariadicParameterSymbol(string name, string documentation, Symbol parent)
            : base(name, documentation, parent, TypeFacts.Variadic)
        {
        }
    }
}