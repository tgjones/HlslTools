using System.ComponentModel.Composition;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class IdentifierDeclarationQuickInfoModelProvider : QuickInfoModelProvider<IdentifierDeclarationNameSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, SourceLocation position, IdentifierDeclarationNameSyntax node)
        {
            if (!node.Name.SourceRange.ContainsOrTouches(position))
                return null;

            if (!node.Name.FileSpan.IsInRootFile)
                return null;

            var symbol = semanticModel.GetSymbol(node);
            if (symbol == null)
                return null;

            return QuickInfoModel.ForSymbol(semanticModel, node.Name.FileSpan, symbol);
        }
    }
}