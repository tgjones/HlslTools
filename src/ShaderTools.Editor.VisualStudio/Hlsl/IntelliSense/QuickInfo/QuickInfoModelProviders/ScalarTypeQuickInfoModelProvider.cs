using System.ComponentModel.Composition;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.QuickInfo.QuickInfoModelProviders
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
                ? new SourceFileSpan(node.TypeTokens[0].Span.File, TextSpan.FromBounds(node.TypeTokens[0].Span.Span.Start, node.TypeTokens[1].Span.Span.End))
                : node.TypeTokens[0].Span;

            return QuickInfoModel.ForSymbol(semanticModel, textSpan, symbol);
        }
    }
}