using ShaderTools.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Host.Mef;

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
