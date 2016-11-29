using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;

namespace ShaderTools.VisualStudio.Hlsl.Navigation.GoToDefinitionProviders
{
    [Export]
    internal sealed class GoToDefinitionProviderService
    {
        [ImportingConstructor]
        public GoToDefinitionProviderService([ImportMany] IEnumerable<IGoToDefinitionProvider> providers)
        {
            Providers = providers.ToImmutableArray();
        }

        public ImmutableArray<IGoToDefinitionProvider> Providers { get; }
    }
}