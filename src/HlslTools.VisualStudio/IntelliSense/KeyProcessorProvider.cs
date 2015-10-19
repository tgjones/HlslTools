using System.ComponentModel.Composition;
using HlslTools.VisualStudio.IntelliSense.Completion;
using HlslTools.VisualStudio.IntelliSense.SignatureHelp;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace HlslTools.VisualStudio.IntelliSense
{
    //[Export(typeof(IKeyProcessorProvider))]
    [Name("KeyProcessorProvider")]
    [ContentType(HlslConstants.ContentTypeName)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class KeyProcessorProvider : IKeyProcessorProvider
    {
        [Import]
        public IIntellisenseSessionStackMapService IntellisenseSessionStackMapService { get; set; }

        [Import]
        public CompletionModelManagerProvider CompletionModelManagerProvider { get; set; }

        [Import]
        public SignatureHelpManagerProvider SignatureHelpManagerProvider { get; set; }

        public KeyProcessor GetAssociatedProcessor(IWpfTextView wpfTextView)
        {
            return wpfTextView.Properties.GetOrCreateSingletonProperty(() =>
            {
                var completionModelManager = CompletionModelManagerProvider.GetCompletionModel(wpfTextView);
                var signatureHelpManager = SignatureHelpManagerProvider.GetSignatureHelpManager(wpfTextView);
                return new HlslKeyProcessor(wpfTextView, IntellisenseSessionStackMapService, completionModelManager, signatureHelpManager);
            });
        }
    }
}