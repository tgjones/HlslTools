using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Host.Mef;
using ILanguageService = Microsoft.CodeAnalysis.Host.ILanguageService;

namespace ShaderTools.CodeAnalysis.Hlsl.LanguageServices
{
    [ExportLanguageServiceFactory(typeof(ISyntaxTreeFactoryService), LanguageNames.Hlsl)]
    internal sealed class HlslSyntaxTreeFactoryServiceFactory : ILanguageServiceFactory
    {
        public ILanguageService CreateLanguageService(HostLanguageServices languageServices)
        {
            return new HlslSyntaxTreeFactoryService(
                languageServices.WorkspaceServices.Workspace,
                languageServices.WorkspaceServices.GetRequiredService<IWorkspaceIncludeFileSystem>());
        }
    }
}
