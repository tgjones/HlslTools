using ShaderTools.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Host.Mef;
using IWorkspaceService = Microsoft.CodeAnalysis.Host.IWorkspaceService;

namespace ShaderTools.CodeAnalysis.Diagnostics
{
    [ExportWorkspaceServiceFactory(typeof(IDiagnosticService))]
    internal sealed class DiagnosticServiceFactory : IWorkspaceServiceFactory
    {
        public IWorkspaceService CreateService(HostWorkspaceServices workspaceServices)
        {
            return new DiagnosticService(workspaceServices.Workspace);
        }
    }
}