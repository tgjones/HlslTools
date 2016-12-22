using System.Linq;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Compilation;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.Hlsl.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Navigation.GoToDefinitionProviders
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