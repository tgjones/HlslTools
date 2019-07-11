using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.SymbolSearch;
using ShaderTools.Utilities.Collections;

namespace ShaderTools.CodeAnalysis.ReferenceHighlighting
{
    [ExportWorkspaceService(typeof(IDocumentHighlightsService))]
    internal sealed class DocumentHighlightsService : IDocumentHighlightsService
    {
        public async Task<ImmutableArray<DocumentHighlights>> GetDocumentHighlightsAsync(Document document, int position, IImmutableSet<Document> documentsToSearch, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (semanticModel == null)
                return ImmutableArray<DocumentHighlights>.Empty;

            var syntaxTree = semanticModel.SyntaxTree;
            var mappedPosition = syntaxTree.MapRootFilePosition(position);

            var symbolSearchService = document.LanguageServices.GetService<ISymbolSearchService>();
            if (symbolSearchService == null)
                return ImmutableArray<DocumentHighlights>.Empty;

            var symbolAtPosition = symbolSearchService.FindSymbol(semanticModel, mappedPosition);
            if (symbolAtPosition == null)
                return ImmutableArray<DocumentHighlights>.Empty;

            return SpecializedCollections.SingletonList(
                new DocumentHighlights(
                    document, 
                    symbolSearchService.FindUsages(semanticModel, symbolAtPosition.Value.Symbol)
                        .Where(s => s.Span.File.IsRootFile)
                        .Select(s => new HighlightSpan(s.Span.Span, MapSymbolSpanKind(s.Kind)))
                        .ToImmutableArray())
                ).ToImmutableArray();
        }

        private static HighlightSpanKind MapSymbolSpanKind(SymbolSpanKind value)
        {
            switch (value)
            {
                case SymbolSpanKind.Definition:
                    return HighlightSpanKind.Definition;

                case SymbolSpanKind.Reference:
                    return HighlightSpanKind.Reference;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }
    }
}
