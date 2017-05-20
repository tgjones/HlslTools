using System.ComponentModel.Composition;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class MatrixTypeQuickInfoModelProvider : QuickInfoModelProvider<MatrixTypeSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, SourceLocation position, MatrixTypeSyntax node)
        {
            if (!node.SourceRange.ContainsOrTouches(position))
                return null;

            if (!node.TypeToken.FileSpan.IsInRootFile)
                return null;

            var symbol = semanticModel.GetSymbol(node);
            if (symbol == null)
                return null;

            return QuickInfoModel.ForSymbol(semanticModel, node.TypeToken.FileSpan, symbol);
        }
    }
}