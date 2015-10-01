using System.ComponentModel.Composition;
using HlslTools.Compilation;
using HlslTools.Syntax;

namespace HlslTools.VisualStudio.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class MacroReferenceQuickInfoModelProvider : QuickInfoModelProvider<SyntaxToken>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, SourceLocation position, SyntaxToken node)
        {
            if (!node.SourceRange.ContainsOrTouches(position))
                return null;

            if (!node.Span.IsInRootFile)
                return null;

            if (node.MacroReference == null)
                return null;

            return new QuickInfoModel(semanticModel, node.MacroReference.Span, $"(macro reference) {node.MacroReference.DefineDirective.ToString(true)}");
        }
    }
}