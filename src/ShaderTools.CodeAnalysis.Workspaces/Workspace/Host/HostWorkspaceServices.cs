using ShaderTools.CodeAnalysis.Properties;
using System;
using System.Collections.Generic;
using ShaderTools.Utilities.Collections;
using IWorkspaceService = Microsoft.CodeAnalysis.Host.IWorkspaceService;

namespace ShaderTools.CodeAnalysis.Host
{
    /// <summary>
    /// Per workspace services provided by the host environment.
    /// </summary>
    public abstract class HostWorkspaceServices
    {
        /// <summary>
        /// The host services this workspace services originated from.
        /// </summary>
        /// <returns></returns>
        public abstract HostServices HostServices { get; }

        /// <summary>
        /// The workspace corresponding to this workspace services instantiation
        /// </summary>
        public abstract Workspace Workspace { get; }

        /// <summary>
        /// Gets a workspace specific service provided by the host identified by the service type. 
        /// If the host does not provide the service, this method returns null.
        /// </summary>
        public abstract TWorkspaceService GetService<TWorkspaceService>() where TWorkspaceService : IWorkspaceService;

        /// <summary>
        /// Gets a workspace specific service provided by the host identified by the service type. 
        /// If the host does not provide the service, this method returns <see cref="InvalidOperationException"/>.
        /// </summary>
        public TWorkspaceService GetRequiredService<TWorkspaceService>() where TWorkspaceService : IWorkspaceService
        {
            var service = GetService<TWorkspaceService>();
            if (service == null)
            {
                throw new InvalidOperationException(string.Format(WorkspacesResources.Service_of_type_0_is_required_to_accomplish_the_task_but_is_not_available_from_the_workspace, typeof(TWorkspaceService).FullName));
            }

            return service;
        }

        /// <summary>
        /// A list of language names for supported language services.
        /// </summary>
        public virtual IEnumerable<string> SupportedLanguages
        {
            get { return SpecializedCollections.EmptyEnumerable<string>(); }
        }

        /// <summary>
        /// Returns true if the language is supported.
        /// </summary>
        public virtual bool IsSupported(string languageName)
        {
            return false;
        }

        /// <summary>
        /// Gets the <see cref="HostLanguageServices"/> for the language name.
        /// </summary>
        public virtual HostLanguageServices GetLanguageServices(string languageName)
        {
            throw new NotSupportedException(string.Format(WorkspacesResources.The_language_0_is_not_supported, languageName));
        }
    }
}
