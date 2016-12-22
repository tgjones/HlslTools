using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    [Export]
    internal sealed class QuickInfoModelProviderService
    {
        [ImportingConstructor]
        public QuickInfoModelProviderService([ImportMany] IEnumerable<IQuickInfoModelProvider> providers)
        {
            Providers = providers.OrderByDescending(x => x.Priority).ToImmutableArray();
        }

        public ImmutableArray<IQuickInfoModelProvider> Providers { get; }
    }
}