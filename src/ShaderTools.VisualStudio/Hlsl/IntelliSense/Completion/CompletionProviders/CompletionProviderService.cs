using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;

namespace ShaderTools.VisualStudio.Hlsl.IntelliSense.Completion.CompletionProviders
{
    [Export]
    internal sealed class CompletionProviderService
    {
        [ImportingConstructor]
        public CompletionProviderService([ImportMany] IEnumerable<ICompletionProvider> providers)
        {
            Providers = providers.ToImmutableArray();
        }

        public ImmutableArray<ICompletionProvider> Providers { get; }
    }
}