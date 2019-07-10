using Microsoft.CodeAnalysis.Host;

namespace ShaderTools.CodeAnalysis.Host.Mef
{
    /// <summary>
    /// A factory that creates instances of a specific <see cref="IWorkspaceService"/>.
    /// 
    /// Implement a <see cref="IWorkspaceServiceFactory"/> when you want to provide <see cref="IWorkspaceService"/> instances that use other services.
    /// </summary>
    public interface IWorkspaceServiceFactory
    {
        /// <summary>
        /// Creates a new <see cref="IWorkspaceService"/> instance.
        /// </summary>
        /// <param name="workspaceServices">The <see cref="HostWorkspaceServices"/> that can be used to access other services.</param>
        IWorkspaceService CreateService(HostWorkspaceServices workspaceServices);
    }
}
