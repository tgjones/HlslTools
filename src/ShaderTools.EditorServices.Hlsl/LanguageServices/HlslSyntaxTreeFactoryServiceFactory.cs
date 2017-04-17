using ShaderTools.EditorServices.Workspace.Host;
using ShaderTools.EditorServices.Workspace.Host.Mef;

namespace ShaderTools.EditorServices.Hlsl.LanguageServices
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
