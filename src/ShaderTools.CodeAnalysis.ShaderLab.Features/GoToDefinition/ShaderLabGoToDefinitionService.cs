using Microsoft.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.GoToDefinition;

namespace ShaderTools.CodeAnalysis.ShaderLab.GoToDefinition
{
    [ExportLanguageService(typeof(IGoToDefinitionService), LanguageNames.ShaderLab)]
    internal sealed class ShaderLabGoToDefinitionService : AbstractGoToDefinitionService
    {
    }
}
