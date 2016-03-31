using System.Linq;
using HlslTools.Compilation;
using HlslTools.Syntax;
using HlslTools.Text;
using HlslTools.VisualStudio.SymbolSearch;

namespace HlslTools.VisualStudio.Navigation.GoToDefinitionProviders
{
    internal abstract class SymbolReferenceGoToDefinitionProvider<T> : GoToDefinitionProvider<T>
        where T : ExpressionSyntax
    {
        protected override TextSpan? CreateTargetSpan(SemanticModel semanticModel, SourceLocation position, T node)
        {
            var nameToken = GetNameToken(node);

            if (!nameToken.SourceRange.ContainsOrTouches(position))
                return null;

            if (!nameToken.Span.IsInRootFile)
                return null;

            var symbol = semanticModel.GetSymbol(node);
            if (symbol == null)
                return null;

            var definition = semanticModel.FindUsages(symbol)
                .FirstOrDefault(x => x.Kind == SymbolSpanKind.Definition);

            if (definition == default(SymbolSpan))
                return null;

            return definition.Span;
        }

        protected abstract SyntaxToken GetNameToken(T node);
    }
}