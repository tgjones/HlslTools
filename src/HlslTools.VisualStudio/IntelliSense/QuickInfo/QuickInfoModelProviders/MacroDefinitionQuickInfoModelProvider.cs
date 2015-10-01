using System.ComponentModel.Composition;
using HlslTools.Compilation;
using HlslTools.Syntax;

namespace HlslTools.VisualStudio.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class MacroDefinitionQuickInfoModelProvider : QuickInfoModelProvider<DefineDirectiveTriviaSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, SourceLocation position, DefineDirectiveTriviaSyntax node)
        {
            if (!node.MacroName.SourceRange.ContainsOrTouches(position))
                return null;

            if (!node.MacroName.Span.IsInRootFile)
                return null;

            return new QuickInfoModel(semanticModel, node.MacroName.Span, $"(macro definition) {node}");
        }
    }
}