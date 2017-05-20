using System.ComponentModel.Composition;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class StructDefinitionQuickInfoModelProvider : QuickInfoModelProvider<StructTypeSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, SourceLocation position, StructTypeSyntax node)
        {
            if (node.Name == null || !node.Name.SourceRange.ContainsOrTouches(position))
                return null;

            if (!node.Name.FileSpan.IsInRootFile)
                return null;

            var symbol = semanticModel.GetDeclaredSymbol(node);
            if (symbol == null)
                return null;

            return QuickInfoModel.ForSymbol(semanticModel, node.Name.FileSpan, symbol);
        }
    }
}