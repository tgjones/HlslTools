using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Symbols.Markup;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Symbols
{
    public interface ISymbol
    {
        SymbolKind Kind { get; }
        string Name { get; }
        string Documentation { get; }
        ISymbol Parent { get; }

        SyntaxTreeBase SourceTree { get; }

        /// <summary>
        /// If this symbol is defined in source code, gets the location.
        /// There might be more than one location, for example for separate function declaration and definition.
        /// </summary>
        ImmutableArray<SourceRange> Locations { get; }

        SymbolMarkup ToMarkup(SymbolDisplayFormat format = SymbolDisplayFormat.QuickInfo);
    }

    public enum SymbolDisplayFormat
    {
        QuickInfo,
        MinimallyQualified,
        MinimallyQualifiedWithoutParameters,
        NavigateTo
    }
}
