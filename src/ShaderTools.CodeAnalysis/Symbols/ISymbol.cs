using ShaderTools.CodeAnalysis.Symbols.Markup;

namespace ShaderTools.CodeAnalysis.Symbols
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
