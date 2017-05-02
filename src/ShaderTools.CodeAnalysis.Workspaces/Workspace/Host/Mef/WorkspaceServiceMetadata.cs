using System;
using System.Collections.Generic;
using ShaderTools.Utilities.Collections;

namespace ShaderTools.CodeAnalysis.Host.Mef
{
    /// <summary>
    /// MEF metadata class used for finding <see cref="IWorkspaceService"/> and <see cref="IWorkspaceServiceFactory"/> exports.
    /// </summary>
    internal class WorkspaceServiceMetadata
    {
        public string ServiceType { get; }

        public WorkspaceServiceMetadata(Type serviceType)
            : this(serviceType.AssemblyQualifiedName)
        {
        }

        public WorkspaceServiceMetadata(IDictionary<string, object> data)
        {
            this.ServiceType = (string) data.GetValueOrDefault("ServiceType");
        }

        public WorkspaceServiceMetadata(string serviceType)
        {
            this.ServiceType = serviceType;
        }
    }
}
