namespace HlslTools.Symbols
{
    public sealed class NamespaceSymbol : Symbol
    {
        internal NamespaceSymbol(string name, Symbol parent)
            : base(SymbolKind.Namespace, name, string.Empty, parent)
        {
        }
    }
}