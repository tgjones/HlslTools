using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Compilation;
using ShaderTools.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.SymbolSearch
{
    internal interface ISymbolSearchService : ILanguageService
    {
        SymbolSpan? FindSymbol(SemanticModelBase semanticModel, SourceLocation position);
        ImmutableArray<SymbolSpan> FindUsages(SemanticModelBase semanticModel, ISymbol symbol);
    }
}
