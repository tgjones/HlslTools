using System.ComponentModel.Composition;
using HlslTools.Syntax;

namespace HlslTools.VisualStudio.Navigation.GoToDefinitionProviders
{
    [Export(typeof(IGoToDefinitionProvider))]
    internal sealed class FieldAccessGoToDefinitionProvider : SymbolReferenceGoToDefinitionProvider<FieldAccessExpressionSyntax>
    {
        protected override SyntaxToken GetNameToken(FieldAccessExpressionSyntax node)
        {
            return node.Name;
        }
    }
}