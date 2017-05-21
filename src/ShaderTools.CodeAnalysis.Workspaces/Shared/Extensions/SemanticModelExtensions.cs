using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using ShaderTools.CodeAnalysis.Collections;
using ShaderTools.CodeAnalysis.Compilation;
using ShaderTools.CodeAnalysis.LanguageServices;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.Utilities.Collections;
using ShaderTools.Utilities.PooledObjects;

namespace ShaderTools.CodeAnalysis.Shared.Extensions
{
    internal struct TokenSemanticInfo
    {
        public static readonly TokenSemanticInfo Empty = new TokenSemanticInfo(
            null, null, ImmutableArray<ISymbol>.Empty, null);

        public readonly ISymbol DeclaredSymbol;
        public readonly IAliasSymbol AliasSymbol;
        public readonly ImmutableArray<ISymbol> ReferencedSymbols;
        public readonly ITypeSymbol Type;

        public TokenSemanticInfo(
            ISymbol declaredSymbol,
            IAliasSymbol aliasSymbol,
            ImmutableArray<ISymbol> referencedSymbols,
            ITypeSymbol type)
        {
            DeclaredSymbol = declaredSymbol;
            AliasSymbol = aliasSymbol;
            ReferencedSymbols = referencedSymbols;
            Type = type;
        }

        public ImmutableArray<ISymbol> GetSymbols(bool includeType)
        {
            var result = ArrayBuilder<ISymbol>.GetInstance();
            result.AddIfNotNull(DeclaredSymbol);
            result.AddIfNotNull(AliasSymbol);
            result.AddRange(ReferencedSymbols);

            if (includeType)
            {
                result.AddIfNotNull(Type);
            }

            return result.ToImmutableAndFree();
        }

        public ISymbol GetAnySymbol(bool includeType)
        {
            return GetSymbols(includeType).FirstOrDefault();
        }
    }

    internal static class SemanticModelExtensions
    {
        public static TokenSemanticInfo GetSemanticInfo(
            this SemanticModelBase semanticModel,
            ISyntaxToken token,
            Workspace workspace,
            CancellationToken cancellationToken)
        {
            var languageServices = workspace.Services.GetLanguageServices(token.Language);
            var syntaxFacts = languageServices.GetService<ISyntaxFactsService>();
            if (!syntaxFacts.IsBindableToken(token))
            {
                return TokenSemanticInfo.Empty;
            }

            var semanticFacts = languageServices.GetService<ISemanticFactsService>();

            return GetSemanticInfo(
                semanticModel, semanticFacts, syntaxFacts,
                token, cancellationToken);
        }

        private static TokenSemanticInfo GetSemanticInfo(
            SemanticModelBase semanticModel,
            ISemanticFactsService semanticFacts,
            ISyntaxFactsService syntaxFacts,
            ISyntaxToken token,
            CancellationToken cancellationToken)
        {
            //var aliasSymbol = semanticModel.GetAliasInfo(token.Parent, cancellationToken);
            IAliasSymbol aliasSymbol = null;

            var bindableParent = syntaxFacts.GetBindableParent(token);
            var type = semanticModel.GetTypeInfo(bindableParent).Type;

            var declaredSymbol = semanticFacts.GetDeclaredSymbol(semanticModel, token, cancellationToken);
            var allSymbols = semanticModel.GetSymbolInfo(bindableParent)
                                          .GetBestOrAllSymbols()
                                          .WhereAsArray(s => !s.Equals(declaredSymbol))
                                          .SelectAsArray(s => s);

            return new TokenSemanticInfo(declaredSymbol, aliasSymbol, allSymbols, type);
        }
    }
}
