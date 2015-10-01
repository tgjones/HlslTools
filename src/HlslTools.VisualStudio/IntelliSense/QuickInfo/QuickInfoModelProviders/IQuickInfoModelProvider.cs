using HlslTools.Compilation;
using HlslTools.Syntax;

namespace HlslTools.VisualStudio.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    internal interface IQuickInfoModelProvider
    {
        QuickInfoModel GetModel(SemanticModel semanticModel, SourceLocation position);
    }
}