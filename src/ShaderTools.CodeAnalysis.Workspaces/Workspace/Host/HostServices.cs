using System.Composition;

namespace ShaderTools.CodeAnalysis.Host
{
    /// <summary>
    /// Services provided by the host environment.
    /// </summary>
    public abstract class HostServices
    {
        /// <summary>
        /// Creates a new workspace service. 
        /// </summary>
        protected internal abstract HostWorkspaceServices CreateWorkspaceServices(Workspace workspace);
    }
}
