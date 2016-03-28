namespace HlslTools.Symbols
{
    public sealed class TechniqueSymbol : Symbol
    {
        public TechniqueSymbol(string name)
            : base(SymbolKind.Technique, name, string.Empty, null)
        {
        }
    }
}