using System.Linq;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.SymbolSearch;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Navigation.GoToDefinitionProviders
{
    internal abstract class SymbolReferenceGoToDefinitionProvider<T> : GoToDefinitionProvider<T>
        where T : ExpressionSyntax
    {
        protected override SourceFileSpan? CreateTargetSpan(Document document, SemanticModel semanticModel, SourceLocation position, T node)
        {
            var nameToken = GetNameToken(node);

            if (!nameToken.SourceRange.ContainsOrTouches(position))
                return null;

            if (!nameToken.Span.IsInRootFile)
                return null;

            var symbol = semanticModel.GetSymbol(node);
            if (symbol == null)
                return null;

            var symbolSearchService = document.LanguageServices.GetService<ISymbolSearchService>();
            if (symbolSearchService == null)
                return null;

            var definition = symbolSearchService.FindUsages(semanticModel, symbol)
                .FirstOrDefault(x => x.Kind == SymbolSpanKind.Definition);

            if (definition == default(SymbolSpan))
                return null;

            return definition.Span;
        }

        protected abstract SyntaxToken GetNameToken(T node);
    }
}