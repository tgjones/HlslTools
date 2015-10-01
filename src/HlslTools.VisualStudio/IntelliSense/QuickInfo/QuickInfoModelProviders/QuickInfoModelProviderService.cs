using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;

namespace HlslTools.VisualStudio.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    [Export]
    internal sealed class QuickInfoModelProviderService
    {
        [ImportingConstructor]
        public QuickInfoModelProviderService([ImportMany] IEnumerable<IQuickInfoModelProvider> providers)
        {
            Providers = providers.ToImmutableArray();
        }

        public ImmutableArray<IQuickInfoModelProvider> Providers { get; }
    }
}