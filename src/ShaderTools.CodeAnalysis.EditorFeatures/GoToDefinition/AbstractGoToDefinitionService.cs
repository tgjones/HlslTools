using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShaderTools.CodeAnalysis.Editor.Host;
using ShaderTools.CodeAnalysis.FindSymbols;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.Utilities.Threading;

namespace ShaderTools.CodeAnalysis.Editor.GoToDefinition
{
    internal abstract class AbstractGoToDefinitionService : IGoToDefinitionService
    {
        private readonly IEnumerable<Lazy<IStreamingFindUsagesPresenter>> _streamingPresenters;

        protected AbstractGoToDefinitionService(IEnumerable<Lazy<IStreamingFindUsagesPresenter>> streamingPresenters)
        {
            _streamingPresenters = streamingPresenters;
        }

        private async Task<ISymbol> FindSymbolAsync(Document document, int position, CancellationToken cancellationToken)
        {
            if (!document.SupportsSemanticModel)
            {
                return null;
            }

            var workspace = document.Workspace;

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var semanticInfo = await SymbolFinder.GetSemanticInfoAtPositionAsync(semanticModel, position, workspace, cancellationToken).ConfigureAwait(false);

            // prefer references to declarations.  It's more likely that the user is attempting to 
            // go to a definition at some other location, rather than the definition they're on.  
            // This can happen when a token is at a location that is both a reference and a definition.
            // For example, on an anonymous type member declaration.
            return semanticInfo.AliasSymbol ??
                   semanticInfo.ReferencedSymbols.FirstOrDefault() ??
                   semanticInfo.DeclaredSymbol ??
                   semanticInfo.Type;
        }

        public bool TryGoToDefinition(Document document, int position, CancellationToken cancellationToken)
        {
            if (TryGoToSyntacticDefinitionAsync(document, position, cancellationToken).WaitAndGetResult(cancellationToken))
            {
                return true;
            }

            // First try to compute the referenced symbol and attempt to go to definition for the symbol.
            var symbol = FindSymbolAsync(document, position, cancellationToken).WaitAndGetResult(cancellationToken);
            if (symbol == null)
            {
                return false;
            }

            return GoToDefinitionHelpers.TryGoToDefinition(symbol,
                document.Workspace,
                _streamingPresenters,
                cancellationToken: cancellationToken);
        }

        protected virtual Task<bool> TryGoToSyntacticDefinitionAsync(Document document, int position, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }
    }
}
