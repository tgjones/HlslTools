using System;
using System.Composition;

namespace ShaderTools.CodeAnalysis.Host.Mef
{
    /// <summary>
    /// Use this attribute to declare a <see cref="IWorkspaceService"/> implementation for inclusion in a MEF-based workspace.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public class ExportWorkspaceServiceAttribute : ExportAttribute
    {
        /// <summary>
        /// The assembly qualified name of the service's type.
        /// </summary>
        public string ServiceType { get; }

        /// <summary>
        /// Declares a <see cref="IWorkspaceService"/> implementation for inclusion in a MEF-based workspace.
        /// </summary>
        /// <param name="serviceType">The type that will be used to retrieve the service from a <see cref="HostWorkspaceServices"/>.</param>
        public ExportWorkspaceServiceAttribute(Type serviceType)
            : base(typeof(IWorkspaceService))
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            this.ServiceType = serviceType.AssemblyQualifiedName;
        }
    }
}
