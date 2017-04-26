using System.ComponentModel.Composition;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class GenericVectorTypeQuickInfoModelProvider : QuickInfoModelProvider<GenericVectorTypeSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, SourceLocation position, GenericVectorTypeSyntax node)
        {
            if (!node.VectorKeyword.SourceRange.ContainsOrTouches(position))
                return null;

            if (!node.VectorKeyword.Span.IsInRootFile)
                return null;

            var symbol = semanticModel.GetSymbol(node);
            if (symbol == null)
                return null;

            return QuickInfoModel.ForSymbol(semanticModel, node.VectorKeyword.Span, symbol);
        }
    }
}