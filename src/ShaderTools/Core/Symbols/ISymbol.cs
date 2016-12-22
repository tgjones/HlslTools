using ShaderTools.Core.Symbols.Markup;

namespace ShaderTools.Core.Symbols
{
    public interface ISymbol
    {
        SymbolKind Kind { get; }
        string Name { get; }
        string Documentation { get; }
        ISymbol Parent { get; }

        SymbolMarkup ToMarkup();
    }
}
