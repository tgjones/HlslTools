using ShaderTools.Core.Syntax;
using ShaderTools.Hlsl.Compilation;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    internal interface IQuickInfoModelProvider
    {
        int Priority { get; }
        QuickInfoModel GetModel(SemanticModel semanticModel, SourceLocation position);
    }
}