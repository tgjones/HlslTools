using System.ComponentModel.Composition;
using HlslTools.Compilation;
using HlslTools.Syntax;
using HlslTools.VisualStudio.Util.SyntaxOutput;

namespace HlslTools.VisualStudio.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class NamespaceQuickInfoModelProvider : QuickInfoModelProvider<NamespaceSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, SourceLocation position, NamespaceSyntax node)
        {
            if (!node.Name.SourceRange.ContainsOrTouches(position))
                return null;

            if (!node.Name.Span.IsInRootFile)
                return null;

            return new QuickInfoModel(semanticModel, node.Name.Span, $"(namespace) {node.Name.GetFullyQualifiedName()}");
        }
    }
}