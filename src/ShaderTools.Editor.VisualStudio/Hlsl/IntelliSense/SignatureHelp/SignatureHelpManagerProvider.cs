using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.SignatureHelp.SignatureHelpModelProviders;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.SignatureHelp
{
    [Export]
    internal sealed class SignatureHelpManagerProvider
    {
        [Import]
        public ISignatureHelpBroker SignatureHelpBroker { get; set; }

        [Import]
        public SignatureHelpModelProviderService SignatureHelpModelProviderService { get; set; }

        public SignatureHelpManager GetSignatureHelpManager(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() => new SignatureHelpManager(textView, SignatureHelpBroker, SignatureHelpModelProviderService));
        }
    }
}