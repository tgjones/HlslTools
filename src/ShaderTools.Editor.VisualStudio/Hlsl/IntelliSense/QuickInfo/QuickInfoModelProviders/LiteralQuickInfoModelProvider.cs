using System.ComponentModel.Composition;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class LiteralQuickInfoModelProvider : QuickInfoModelProvider<LiteralExpressionSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, SourceLocation position, LiteralExpressionSyntax node)
        {
            if (!node.Token.SourceRange.ContainsOrTouches(position))
                return null;

            if (!node.Token.Span.IsInRootFile)
                return null;

            var symbol = semanticModel.GetExpressionType(node);
            if (symbol == null)
                return null;

            return QuickInfoModel.ForSymbol(semanticModel, node.Token.Span, symbol);
        }
    }
}