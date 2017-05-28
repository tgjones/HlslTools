using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShaderTools.CodeAnalysis.FindSymbols;
using ShaderTools.CodeAnalysis.FindUsages;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.Utilities.Collections;

namespace ShaderTools.CodeAnalysis.GoToDefinition
{
    internal abstract class AbstractGoToDefinitionService : IGoToDefinitionService
    {
        public async Task<ImmutableArray<DefinitionItem>> FindDefinitionsAsync(Document document, int position, CancellationToken cancellationToken)
        {
            var syntacticDefinitions = await GetSyntacticDefinitionsAsync(document, position, cancellationToken);
            if (!syntacticDefinitions.IsEmpty)
            {
                return syntacticDefinitions;
            }

            // First try to compute the referenced symbol and attempt to go to definition for the symbol.
            var symbol = await FindSymbolAsync(document, position, cancellationToken);
            if (symbol == null)
            {
                return ImmutableArray<DefinitionItem>.Empty;
            }

            var definition = await SymbolFinder.FindSourceDefinitionAsync(symbol, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            symbol = definition ?? symbol;

            if (symbol.Locations.IsEmpty)
            {
                return ImmutableArray<DefinitionItem>.Empty;
            }

            var workspace = document.Workspace;
            var options = workspace.Options;

            return ImmutableArray.Create(symbol.ToDefinitionItem(workspace));
        }

        protected virtual Task<ImmutableArray<DefinitionItem>> GetSyntacticDefinitionsAsync(Document document, int position, CancellationToken cancellationToken)
        {
            return Task.FromResult(ImmutableArray<DefinitionItem>.Empty);
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
    }
}
