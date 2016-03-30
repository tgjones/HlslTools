using System.ComponentModel.Composition;
using System.Linq;
using HlslTools.Compilation;
using HlslTools.Syntax;
using HlslTools.Text;

namespace HlslTools.VisualStudio.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class ScalarTypeQuickInfoModelProvider : QuickInfoModelProvider<ScalarTypeSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, SourceLocation position, ScalarTypeSyntax node)
        {
            if (!node.SourceRange.ContainsOrTouches(position))
                return null;

            if (node.TypeTokens.Any(x => !x.Span.IsInRootFile))
                return null;

            var symbol = semanticModel.GetSymbol(node);
            if (symbol == null)
                return null;

            var textSpan = (node.TypeTokens.Count > 1)
                ? TextSpan.Union(node.TypeTokens[0].Span, node.TypeTokens[1].Span)
                : node.TypeTokens[0].Span;

            return QuickInfoModel.ForSymbol(semanticModel, textSpan, symbol);
        }
    }
}