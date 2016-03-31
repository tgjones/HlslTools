using System.ComponentModel.Composition;
using HlslTools.Syntax;

namespace HlslTools.VisualStudio.Navigation.GoToDefinitionProviders
{
    [Export(typeof(IGoToDefinitionProvider))]
    internal sealed class FunctionInvocationGoToDefinitionProvider : SymbolReferenceGoToDefinitionProvider<FunctionInvocationExpressionSyntax>
    {
        protected override SyntaxToken GetNameToken(FunctionInvocationExpressionSyntax node)
        {
            return node.Name.GetUnqualifiedName().Name;
        }
    }
}