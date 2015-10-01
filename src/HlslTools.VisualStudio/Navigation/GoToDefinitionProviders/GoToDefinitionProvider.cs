using System.Linq;
using HlslTools.Compilation;
using HlslTools.Syntax;
using HlslTools.Text;

namespace HlslTools.VisualStudio.Navigation.GoToDefinitionProviders
{
    internal abstract class GoToDefinitionProvider<T> : IGoToDefinitionProvider
        where T : SyntaxNode
    {
        public TextSpan? GetTargetSpan(SemanticModel semanticModel, SourceLocation position)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            return syntaxTree.Root
                .FindStartTokens(position, true)
                .Select(token => token.AncestorsAndSelf().OfType<T>().FirstOrDefault())
                .Where(t => t != null)
                .Select(t => CreateTargetSpan(semanticModel, position, t))
                .FirstOrDefault();
        }

        protected abstract TextSpan? CreateTargetSpan(SemanticModel semanticModel, SourceLocation position, T node);
    }
}