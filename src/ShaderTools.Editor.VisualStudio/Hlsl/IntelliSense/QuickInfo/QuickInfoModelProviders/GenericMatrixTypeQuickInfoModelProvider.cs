using System.ComponentModel.Composition;
using ShaderTools.Core.Syntax;
using ShaderTools.Hlsl.Compilation;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class GenericMatrixTypeQuickInfoModelProvider : QuickInfoModelProvider<GenericMatrixTypeSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, SourceLocation position, GenericMatrixTypeSyntax node)
        {
            if (!node.MatrixKeyword.SourceRange.ContainsOrTouches(position))
                return null;

            if (!node.MatrixKeyword.Span.IsInRootFile)
                return null;

            var symbol = semanticModel.GetSymbol(node);
            if (symbol == null)
                return null;

            return QuickInfoModel.ForSymbol(semanticModel, node.MatrixKeyword.Span, symbol);
        }
    }
}