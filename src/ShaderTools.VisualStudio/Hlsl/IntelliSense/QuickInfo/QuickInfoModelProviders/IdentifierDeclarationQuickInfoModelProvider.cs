using System.ComponentModel.Composition;
using ShaderTools.Hlsl.Compilation;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.VisualStudio.Hlsl.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class IdentifierDeclarationQuickInfoModelProvider : QuickInfoModelProvider<IdentifierDeclarationNameSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, SourceLocation position, IdentifierDeclarationNameSyntax node)
        {
            if (!node.Name.SourceRange.ContainsOrTouches(position))
                return null;

            if (!node.Name.Span.IsInRootFile)
                return null;

            var symbol = semanticModel.GetSymbol(node);
            if (symbol == null)
                return null;

            return QuickInfoModel.ForSymbol(semanticModel, node.Name.Span, symbol);
        }
    }
}