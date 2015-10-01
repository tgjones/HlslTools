using System.ComponentModel.Composition;
using HlslTools.Compilation;
using HlslTools.Syntax;

namespace HlslTools.VisualStudio.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class TechniqueQuickInfoModelProvider : QuickInfoModelProvider<TechniqueSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, SourceLocation position, TechniqueSyntax node)
        {
            if (!node.Name.SourceRange.ContainsOrTouches(position))
                return null;

            if (!node.Name.Span.IsInRootFile)
                return null;

            return new QuickInfoModel(semanticModel, node.Name.Span, $"(technique) {node.Name}");
        }
    }
}