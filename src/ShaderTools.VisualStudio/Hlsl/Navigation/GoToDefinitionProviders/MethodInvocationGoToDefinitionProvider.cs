using System.ComponentModel.Composition;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.VisualStudio.Hlsl.Navigation.GoToDefinitionProviders
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