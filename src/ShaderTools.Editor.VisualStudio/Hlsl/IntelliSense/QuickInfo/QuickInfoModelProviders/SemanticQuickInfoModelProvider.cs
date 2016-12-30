using System.ComponentModel.Composition;
using ShaderTools.Core.Syntax;
using ShaderTools.Hlsl.Compilation;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class SemanticQuickInfoModelProvider : QuickInfoModelProvider<SemanticSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, SourceLocation position, SemanticSyntax node)
        {
            if (!node.Semantic.SourceRange.ContainsOrTouches(position))
                return null;

            if (!node.Semantic.Span.IsInRootFile)
                return null;

            var symbol = semanticModel.GetSymbol(node);
            if (symbol == null)
                return null;

            return QuickInfoModel.ForSymbol(semanticModel, node.Semantic.Span, symbol);
        }
    }
}