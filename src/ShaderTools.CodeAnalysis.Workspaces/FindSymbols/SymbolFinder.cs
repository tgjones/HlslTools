using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShaderTools.CodeAnalysis.Compilation;
using ShaderTools.CodeAnalysis.Shared.Extensions;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.FindSymbols
{
    internal static class SymbolFinder
    {
        internal static async Task<TokenSemanticInfo> GetSemanticInfoAtPositionAsync(
            SemanticModelBase semanticModel,
            int position,
            Workspace workspace,
            CancellationToken cancellationToken)
        {
            var syntaxTree = semanticModel.SyntaxTree;
            var sourceLocation = syntaxTree.MapRootFilePosition(position);
            var syntaxFacts = workspace.Services.GetLanguageServices(semanticModel.Language).GetService<ISyntaxFactsService>();
            var token = await syntaxTree.GetTouchingTokenAsync(sourceLocation, syntaxFacts.IsBindableToken, cancellationToken, findInsideTrivia: true).ConfigureAwait(false);

            if (token != null &&
                token.FileSpan.Span.IntersectsWith(position))
            {
                return semanticModel.GetSemanticInfo(token, workspace, cancellationToken);
            }

            return TokenSemanticInfo.Empty;
        }

        /// <summary>
        /// Finds the definition symbol declared in source code for a corresponding reference symbol. 
        /// Returns null if no such symbol can be found
        /// </summary>
        public static async Task<ISymbol> FindSourceDefinitionAsync(
            ISymbol symbol, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!InSource(symbol))
            {
                return null;
            }

            return symbol;
        }

        private static bool InSource(ISymbol symbol)
        {
            if (symbol == null)
            {
                return false;
            }

            return symbol.Locations.Any();
        }
    }
}
