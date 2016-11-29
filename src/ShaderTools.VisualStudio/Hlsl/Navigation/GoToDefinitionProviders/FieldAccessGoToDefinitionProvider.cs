using System.ComponentModel.Composition;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.VisualStudio.Hlsl.Navigation.GoToDefinitionProviders
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