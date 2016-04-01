namespace HlslTools.Symbols
{
    public sealed class VariadicParameterSymbol : ParameterSymbol
    {
        internal VariadicParameterSymbol(string name, string documentation, Symbol parent)
            : base(name, documentation, parent, TypeFacts.Variadic)
        {
        }
    }
}