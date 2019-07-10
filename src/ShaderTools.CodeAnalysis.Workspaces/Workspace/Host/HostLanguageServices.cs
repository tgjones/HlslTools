using System;
using Microsoft.CodeAnalysis.Host;

namespace ShaderTools.CodeAnalysis.Host
{
    /// <summary>
    /// Per language services provided by the host environment.
    /// </summary>
    public abstract class HostLanguageServices
    {
        /// <summary>
        /// The <see cref="HostWorkspaceServices"/> that originated this language service.
        /// </summary>
        public abstract HostWorkspaceServices WorkspaceServices { get; }

        /// <summary>
        /// The name of the language
        /// </summary>
        public abstract string Language { get; }

        /// <summary>
        /// Gets a language specific service provided by the host identified by the service type. 
        /// If the host does not provide the service, this method returns null.
        /// </summary>
        public abstract TLanguageService GetService<TLanguageService>() where TLanguageService : ILanguageService;

        /// <summary>
        /// Gets a language specific service provided by the host identified by the service type. 
        /// If the host does not provide the service, this method returns throws <see cref="InvalidOperationException"/>.
        /// </summary>
        public TLanguageService GetRequiredService<TLanguageService>() where TLanguageService : ILanguageService
        {
            var service = GetService<TLanguageService>();
            if (service == null)
            {
                throw new InvalidOperationException();
            }

            return service;
        }
    }
}