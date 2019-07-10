using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Shared.Extensions;
using ShaderTools.CodeAnalysis.Shared.Utilities;

namespace ShaderTools.CodeAnalysis.QuickInfo
{
    internal abstract class QuickInfoService : ILanguageService
    {
        public static QuickInfoService GetService(Document document)
            => document?.GetLanguageService<QuickInfoService>();

        private readonly Workspace _workspace;
        private readonly string _language;
        private ImmutableArray<IQuickInfoProvider> _providers;

        protected QuickInfoService(Workspace workspace, string language)
        {
            _workspace = workspace;
            _language = language;
        }

        private ImmutableArray<IQuickInfoProvider> GetProviders()
        {
            if (_providers.IsDefault)
            {
                var mefExporter = (IMefHostExportProvider)_workspace.Services.HostServices;

                var providers = ExtensionOrderer
                    .Order(mefExporter.GetExports<IQuickInfoProvider, OrderableLanguageMetadata>()
                        .Where(lz => lz.Metadata.Language == _language))
                    .Select(lz => lz.Value)
                    .ToImmutableArray();

                ImmutableInterlocked.InterlockedCompareExchange(ref _providers, providers, default);
            }

            return _providers;
        }

        public async Task<QuickInfoItem> GetQuickInfoAsync(Document document, int position, CancellationToken cancellationToken)
        {
            foreach (var provider in GetProviders())
            {
                var info = await provider.GetItemAsync(document, position, cancellationToken).ConfigureAwait(false);
                if (info != null)
                {
                    return info;
                }
            }

            return null;
        }
    }
}
