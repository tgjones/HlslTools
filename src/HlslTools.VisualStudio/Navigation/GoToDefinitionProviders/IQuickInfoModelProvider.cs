using HlslTools.Compilation;
using HlslTools.Syntax;
using HlslTools.Text;

namespace HlslTools.VisualStudio.Navigation.GoToDefinitionProviders
{
    internal interface IGoToDefinitionProvider
    {
        TextSpan? GetTargetSpan(SemanticModel semanticModel, SourceLocation position);
    }
}