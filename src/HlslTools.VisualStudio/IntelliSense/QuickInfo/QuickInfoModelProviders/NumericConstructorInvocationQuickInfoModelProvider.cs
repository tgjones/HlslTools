using System.ComponentModel.Composition;
using HlslTools.Compilation;
using HlslTools.Syntax;

namespace HlslTools.VisualStudio.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class NumericConstructorInvocationQuickInfoModelProvider : QuickInfoModelProvider<NumericConstructorInvocationExpressionSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, SourceLocation position, NumericConstructorInvocationExpressionSyntax node)
        {
            if (!node.Type.SourceRange.ContainsOrTouches(position))
                return null;

            if (!node.Type.GetTextSpanSafe().IsInRootFile)
                return null;

            var symbol = semanticModel.GetSymbol(node);
            if (symbol == null)
                return null;

            return QuickInfoModel.ForSymbol(semanticModel, node.Type.GetTextSpanRoot(), symbol);
        }
    }
}