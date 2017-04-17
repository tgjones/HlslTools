using ShaderTools.EditorServices.Workspace.Host;
using ShaderTools.EditorServices.Workspace.Host.Mef;
using ShaderTools.Hlsl.Text;

namespace ShaderTools.EditorServices.Hlsl.LanguageServices
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
