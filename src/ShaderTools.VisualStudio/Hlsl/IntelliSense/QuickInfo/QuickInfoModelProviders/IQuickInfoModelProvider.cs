using ShaderTools.Hlsl.Compilation;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.VisualStudio.Hlsl.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    internal interface IQuickInfoModelProvider
    {
        int Priority { get; }
        QuickInfoModel GetModel(SemanticModel semanticModel, SourceLocation position);
    }
}