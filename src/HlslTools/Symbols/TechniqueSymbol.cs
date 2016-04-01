namespace HlslTools.Symbols
{
    public sealed class TechniqueSymbol : Symbol
    {
        internal TechniqueSymbol(string name)
            : base(SymbolKind.Technique, name, string.Empty, null)
        {
        }
    }
}