using System.ComponentModel.Composition;
using HlslTools.Syntax;

namespace HlslTools.VisualStudio.Navigation.GoToDefinitionProviders
{
    [Export(typeof(IGoToDefinitionProvider))]
    internal sealed class IdentifierGoToDefinitionProvider : SymbolReferenceGoToDefinitionProvider<IdentifierNameSyntax>
    {
        protected override SyntaxToken GetNameToken(IdentifierNameSyntax node)
        {
            return node.Name;
        }
    }
}