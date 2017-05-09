using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Navigation.GoToDefinitionProviders
{
    internal interface IGoToDefinitionProvider
    {
        SourceFileSpan? GetTargetSpan(Document document, SemanticModel semanticModel, SourceLocation position);
    }
}