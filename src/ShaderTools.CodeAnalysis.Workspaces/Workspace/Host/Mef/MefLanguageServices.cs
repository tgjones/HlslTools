using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.Host;

namespace ShaderTools.CodeAnalysis.Host.Mef
{
    internal class MefLanguageServices : HostLanguageServices
    {
        private readonly MefWorkspaceServices _workspaceServices;
        private readonly string _language;
        private readonly ImmutableArray<Lazy<ILanguageService, LanguageServiceMetadata>> _services;

        private ImmutableDictionary<Type, Lazy<ILanguageService, LanguageServiceMetadata>> _serviceMap
            = ImmutableDictionary<Type, Lazy<ILanguageService, LanguageServiceMetadata>>.Empty;

        public MefLanguageServices(
            MefWorkspaceServices workspaceServices,
            string language)
        {
            _workspaceServices = workspaceServices;
            _language = language;

            var hostServices = workspaceServices.HostExportProvider;

            _services = hostServices.GetExports<ILanguageService, LanguageServiceMetadata>()
                    .Concat(hostServices.GetExports<ILanguageServiceFactory, LanguageServiceMetadata>()
                                        .Select(lz => new Lazy<ILanguageService, LanguageServiceMetadata>(() => lz.Value.CreateLanguageService(this), lz.Metadata)))
                    .Where(lz => lz.Metadata.Language == language).ToImmutableArray();
        }

        public override HostWorkspaceServices WorkspaceServices => _workspaceServices;

        public override string Language => _language;

        public bool HasServices
        {
            get { return _services.Length > 0; }
        }

        public override TLanguageService GetService<TLanguageService>()
        {
            if (TryGetService(typeof(TLanguageService), out var service))
            {
                return (TLanguageService) service.Value;
            }
            else
            {
                return default(TLanguageService);
            }
        }

        internal bool TryGetService(Type serviceType, out Lazy<ILanguageService, LanguageServiceMetadata> service)
        {
            if (!_serviceMap.TryGetValue(serviceType, out service))
            {
                service = ImmutableInterlocked.GetOrAdd(ref _serviceMap, serviceType, svctype =>
                {
                    // PERF: Hoist AssemblyQualifiedName out of inner lambda to avoid repeated string allocations.
                    var assemblyQualifiedName = svctype.AssemblyQualifiedName;
                    return PickLanguageService(_services.Where(lz => lz.Metadata.ServiceType == assemblyQualifiedName));
                });
            }

            return service != default(Lazy<ILanguageService, LanguageServiceMetadata>);
        }

        private Lazy<ILanguageService, LanguageServiceMetadata> PickLanguageService(IEnumerable<Lazy<ILanguageService, LanguageServiceMetadata>> services)
        {
            return services.SingleOrDefault();
        }
    }
}
