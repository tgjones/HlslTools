using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.Host;

namespace ShaderTools.CodeAnalysis.Host.Mef
{
    internal class MefWorkspaceServices : HostWorkspaceServices
    {
        private readonly IMefHostExportProvider _exportProvider;
        private readonly Workspace _workspace;

        private readonly ImmutableArray<Lazy<IWorkspaceService, WorkspaceServiceMetadata>> _services;

        // map of type name to workspace service
        private ImmutableDictionary<Type, Lazy<IWorkspaceService, WorkspaceServiceMetadata>> _serviceMap
            = ImmutableDictionary<Type, Lazy<IWorkspaceService, WorkspaceServiceMetadata>>.Empty;

        // accumulated cache for language services
        private ImmutableDictionary<string, MefLanguageServices> _languageServicesMap
            = ImmutableDictionary<string, MefLanguageServices>.Empty;

        public MefWorkspaceServices(IMefHostExportProvider host, Workspace workspace)
        {
            _exportProvider = host;
            _workspace = workspace;
            _services = host.GetExports<IWorkspaceService, WorkspaceServiceMetadata>()
                .Concat(host.GetExports<IWorkspaceServiceFactory, WorkspaceServiceMetadata>()
                            .Select(lz => new Lazy<IWorkspaceService, WorkspaceServiceMetadata>(() => lz.Value.CreateService(this), lz.Metadata)))
                .ToImmutableArray();
        }

        public override HostServices HostServices
        {
            get { return (HostServices) _exportProvider; }
        }

        internal IMefHostExportProvider HostExportProvider => _exportProvider;

        public override Workspace Workspace => _workspace;

        public override TWorkspaceService GetService<TWorkspaceService>()
        {
            if (TryGetService(typeof(TWorkspaceService), out var service))
            {
                return (TWorkspaceService) service.Value;
            }
            else
            {
                return default(TWorkspaceService);
            }
        }

        private bool TryGetService(Type serviceType, out Lazy<IWorkspaceService, WorkspaceServiceMetadata> service)
        {
            if (!_serviceMap.TryGetValue(serviceType, out service))
            {
                service = ImmutableInterlocked.GetOrAdd(ref _serviceMap, serviceType, svctype =>
                {
                    // Pick from list of exported factories and instances
                    // PERF: Hoist AssemblyQualifiedName out of inner lambda to avoid repeated string allocations.
                    var assemblyQualifiedName = svctype.AssemblyQualifiedName;
                    return PickWorkspaceService(_services.Where(lz => lz.Metadata.ServiceType == assemblyQualifiedName));
                });
            }

            return service != default(Lazy<IWorkspaceService, WorkspaceServiceMetadata>);
        }

        private Lazy<IWorkspaceService, WorkspaceServiceMetadata> PickWorkspaceService(IEnumerable<Lazy<IWorkspaceService, WorkspaceServiceMetadata>> services)
        {
            return services.SingleOrDefault();
        }

        private IEnumerable<string> _languages;

        private IEnumerable<string> GetSupportedLanguages()
        {
            if (_languages == null)
            {
                var list = _exportProvider.GetExports<ILanguageService, LanguageServiceMetadata>().Select(lz => lz.Metadata.Language).Concat(
                           _exportProvider.GetExports<ILanguageServiceFactory, LanguageServiceMetadata>().Select(lz => lz.Metadata.Language))
                           .Distinct()
                           .ToImmutableArray();

                Interlocked.CompareExchange(ref _languages, list, null);
            }

            return _languages;
        }

        public override IEnumerable<string> SupportedLanguages
        {
            get { return this.GetSupportedLanguages(); }
        }

        public override bool IsSupported(string languageName)
        {
            languageName = languageName.ToUpperInvariant();

            return this.GetSupportedLanguages().Contains(languageName);
        }

        public override HostLanguageServices GetLanguageServices(string languageName)
        {
            languageName = languageName.ToUpperInvariant();

            var currentServicesMap = _languageServicesMap;
            if (!currentServicesMap.TryGetValue(languageName, out var languageServices))
            {
                languageServices = ImmutableInterlocked.GetOrAdd(ref _languageServicesMap, languageName, _ => new MefLanguageServices(this, languageName));
            }

            if (languageServices.HasServices)
            {
                return languageServices;
            }
            else
            {
                // throws exception
                return base.GetLanguageServices(languageName);
            }
        }
    }
}
