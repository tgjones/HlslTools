using System.ComponentModel.Composition;
using HlslTools.Compilation;
using HlslTools.Syntax;
using HlslTools.VisualStudio.Util.SyntaxOutput;

namespace HlslTools.VisualStudio.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class StructDefinitionQuickInfoModelProvider : QuickInfoModelProvider<StructTypeSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, SourceLocation position, StructTypeSyntax node)
        {
            if (!node.Name.SourceRange.ContainsOrTouches(position))
                return null;

            if (!node.Name.Span.IsInRootFile)
                return null;

            return new QuickInfoModel(semanticModel, node.Name.Span, $"(struct) {node.Name.GetFullyQualifiedName()}");
        }
    }
}