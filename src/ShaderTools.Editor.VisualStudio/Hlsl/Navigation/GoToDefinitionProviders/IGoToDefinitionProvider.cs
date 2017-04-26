using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Navigation.GoToDefinitionProviders
{
    internal interface IGoToDefinitionProvider
    {
        TextSpan? GetTargetSpan(SemanticModel semanticModel, SourceLocation position);
    }
}