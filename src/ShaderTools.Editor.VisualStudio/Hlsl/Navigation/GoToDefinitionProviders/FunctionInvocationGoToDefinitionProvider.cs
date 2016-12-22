using System.ComponentModel.Composition;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Navigation.GoToDefinitionProviders
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