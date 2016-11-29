using System.ComponentModel.Composition;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.VisualStudio.Hlsl.Navigation.GoToDefinitionProviders
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