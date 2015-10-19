using HlslTools.Compilation;
using HlslTools.Syntax;

namespace HlslTools.VisualStudio.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    internal interface IQuickInfoModelProvider
    {
        int Priority { get; }
        QuickInfoModel GetModel(SemanticModel semanticModel, SourceLocation position);
    }
}