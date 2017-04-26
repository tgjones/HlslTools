using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    internal interface IQuickInfoModelProvider
    {
        int Priority { get; }
        QuickInfoModel GetModel(SemanticModel semanticModel, SourceLocation position);
    }
}