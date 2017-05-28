using ShaderTools.CodeAnalysis.GoToDefinition;
using ShaderTools.CodeAnalysis.Host.Mef;

namespace ShaderTools.CodeAnalysis.ShaderLab.GoToDefinition
{
    [ExportLanguageService(typeof(IGoToDefinitionService), LanguageNames.ShaderLab)]
    internal sealed class ShaderLabGoToDefinitionService : AbstractGoToDefinitionService
    {
    }
}
