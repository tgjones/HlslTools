using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Navigation.GoToDefinitionProviders
{
    internal abstract class GoToDefinitionProvider<T> : IGoToDefinitionProvider
        where T : SyntaxNode
    {
        public SourceFileSpan? GetTargetSpan(SemanticModel semanticModel, SourceLocation position)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            return ((SyntaxNode) syntaxTree.Root)
                .FindStartTokens(position, true)
                .Select(token => token.AncestorsAndSelf().OfType<T>().FirstOrDefault())
                .Where(t => t != null)
                .Select(t => CreateTargetSpan(semanticModel, position, t))
                .FirstOrDefault();
        }

        protected abstract SourceFileSpan? CreateTargetSpan(SemanticModel semanticModel, SourceLocation position, T node);
    }
}