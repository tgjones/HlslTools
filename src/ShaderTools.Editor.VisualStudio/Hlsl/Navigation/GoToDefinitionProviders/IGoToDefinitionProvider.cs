using ShaderTools.Core.Syntax;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Compilation;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Navigation.GoToDefinitionProviders
{
    internal interface IGoToDefinitionProvider
    {
        TextSpan? GetTargetSpan(SemanticModel semanticModel, SourceLocation position);
    }
}