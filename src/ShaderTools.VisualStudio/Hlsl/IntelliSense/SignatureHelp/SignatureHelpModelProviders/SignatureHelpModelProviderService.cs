using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;

namespace ShaderTools.VisualStudio.Hlsl.IntelliSense.SignatureHelp.SignatureHelpModelProviders
{
    [Export]
    internal sealed class SignatureHelpModelProviderService
    {
        [ImportingConstructor]
        public SignatureHelpModelProviderService([ImportMany] IEnumerable<ISignatureHelpModelProvider> providers)
        {
            Providers = providers.ToImmutableArray();
        }

        public ImmutableArray<ISignatureHelpModelProvider> Providers { get; }
    }
}