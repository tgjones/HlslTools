using System.ComponentModel.Composition;
using HlslTools.Compilation;
using HlslTools.Syntax;

namespace HlslTools.VisualStudio.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class FunctionInvocationQuickInfoModelProvider : QuickInfoModelProvider<FunctionInvocationExpressionSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, SourceLocation position, FunctionInvocationExpressionSyntax node)
        {
            var actualName = node.Name.GetUnqualifiedName().Name;

            if (!actualName.SourceRange.ContainsOrTouches(position))
                return null;

            if (!actualName.Span.IsInRootFile)
                return null;

            var symbol = semanticModel.GetSymbol(node);
            if (symbol == null)
                return null;

            return QuickInfoModel.ForSymbol(semanticModel, actualName.Span, symbol);
        }
    }
}