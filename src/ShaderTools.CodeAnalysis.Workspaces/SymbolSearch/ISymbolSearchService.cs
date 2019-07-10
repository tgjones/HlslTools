using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Compilation;
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
