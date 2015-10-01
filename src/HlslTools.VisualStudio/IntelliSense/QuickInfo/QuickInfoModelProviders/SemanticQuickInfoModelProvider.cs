using System.ComponentModel.Composition;
using HlslTools.Compilation;
using HlslTools.Syntax;

namespace HlslTools.VisualStudio.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class SemanticQuickInfoModelProvider : QuickInfoModelProvider<SemanticSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, SourceLocation position, SemanticSyntax node)
        {
            if (!node.Semantic.SourceRange.ContainsOrTouches(position))
                return null;

            if (!node.Semantic.Span.IsInRootFile)
                return null;

            return new QuickInfoModel(semanticModel, node.Semantic.Span, $"(semantic) {node.Semantic}");
        }
    }
}