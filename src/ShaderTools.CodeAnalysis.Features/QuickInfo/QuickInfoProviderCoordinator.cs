using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace ShaderTools.CodeAnalysis.QuickInfo
{
    [Export(typeof(QuickInfoProviderCoordinator))]
    internal sealed class QuickInfoProviderCoordinator : IQuickInfoProviderCoordinator
    {
        private readonly IList<IQuickInfoProvider> _providers;

        public QuickInfoProviderCoordinator(IList<IQuickInfoProvider> providers)
        {
            _providers = providers;
        }

        public async Task<(QuickInfoItem, IQuickInfoProvider)> GetItemAsync(Document document, int position, CancellationToken cancellationToken)
        {
            foreach (var provider in _providers)
            {
                var item = await provider.GetItemAsync(document, position, cancellationToken).ConfigureAwait(false);
                if (item != null)
                {
                    return (item, provider);
                }
            }

            return ((QuickInfoItem) null, (IQuickInfoProvider) null);
        }
    }
}