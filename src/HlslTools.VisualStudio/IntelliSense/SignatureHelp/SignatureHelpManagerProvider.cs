using System.ComponentModel.Composition;
using HlslTools.VisualStudio.IntelliSense.SignatureHelp.SignatureHelpModelProviders;
using HlslTools.VisualStudio.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;

namespace HlslTools.VisualStudio.IntelliSense.SignatureHelp
{
    [Export]
    internal sealed class SignatureHelpManagerProvider
    {
        [Import]
        public ISignatureHelpBroker SignatureHelpBroker { get; set; }

        [Import]
        public SignatureHelpModelProviderService SignatureHelpModelProviderService { get; set; }

        [Import]
        public VisualStudioSourceTextFactory SourceTextFactory { get; set; }

        public SignatureHelpManager GetSignatureHelpManager(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() => new SignatureHelpManager(textView, SignatureHelpBroker, SignatureHelpModelProviderService, SourceTextFactory));
        }
    }
}