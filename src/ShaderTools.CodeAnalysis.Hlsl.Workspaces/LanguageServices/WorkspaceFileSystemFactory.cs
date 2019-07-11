using ShaderTools.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Host.Mef;
using IWorkspaceService = Microsoft.CodeAnalysis.Host.IWorkspaceService;

namespace ShaderTools.CodeAnalysis.Hlsl.LanguageServices
{
    [ExportWorkspaceServiceFactory(typeof(IWorkspaceIncludeFileSystem))]
    internal sealed class WorkspaceFileSystemFactory : IWorkspaceServiceFactory
    {
        public IWorkspaceService CreateService(HostWorkspaceServices workspaceServices)
        {
            return new WorkspaceFileSystem(workspaceServices.Workspace);
        }
    }
}
