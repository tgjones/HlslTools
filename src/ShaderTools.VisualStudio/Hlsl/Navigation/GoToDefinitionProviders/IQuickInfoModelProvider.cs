using ShaderTools.Hlsl.Compilation;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.Hlsl.Text;

namespace ShaderTools.VisualStudio.Hlsl.Navigation.GoToDefinitionProviders
{
    internal interface IGoToDefinitionProvider
    {
        TextSpan? GetTargetSpan(SemanticModel semanticModel, SourceLocation position);
    }
}