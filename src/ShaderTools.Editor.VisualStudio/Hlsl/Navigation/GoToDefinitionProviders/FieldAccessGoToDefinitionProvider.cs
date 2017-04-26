using System.ComponentModel.Composition;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Navigation.GoToDefinitionProviders
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