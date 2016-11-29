using System.Linq;
using ShaderTools.Hlsl.Compilation;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.VisualStudio.Hlsl.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    internal abstract class QuickInfoModelProvider<T> : IQuickInfoModelProvider
        where T : SyntaxNode
    {
        public virtual int Priority { get; } = 0;

        public QuickInfoModel GetModel(SemanticModel semanticModel, SourceLocation position)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            return syntaxTree.Root
                .FindStartTokens(position, true)
                .Select(token => token.AncestorsAndSelf().OfType<T>().FirstOrDefault())
                .Where(t => t != null)
                .Select(t => CreateModel(semanticModel, position, t))
                .FirstOrDefault();
        }

        protected abstract QuickInfoModel CreateModel(SemanticModel semanticModel, SourceLocation position, T node);
    }
}