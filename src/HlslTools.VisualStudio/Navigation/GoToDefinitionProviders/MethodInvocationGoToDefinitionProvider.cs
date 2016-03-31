using System.ComponentModel.Composition;
using HlslTools.Syntax;

namespace HlslTools.VisualStudio.Navigation.GoToDefinitionProviders
{
    [Export(typeof(IGoToDefinitionProvider))]
    internal sealed class MethodInvocationGoToDefinitionProvider : SymbolReferenceGoToDefinitionProvider<MethodInvocationExpressionSyntax>
    {
        protected override SyntaxToken GetNameToken(MethodInvocationExpressionSyntax node)
        {
            return node.Name;
        }
    }
}