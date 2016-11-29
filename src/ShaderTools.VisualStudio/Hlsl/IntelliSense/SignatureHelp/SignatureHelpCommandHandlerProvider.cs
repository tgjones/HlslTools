using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace ShaderTools.VisualStudio.Hlsl.IntelliSense.SignatureHelp
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType(HlslConstants.ContentTypeName)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class SignatureHelpCommandHandlerProvider : IVsTextViewCreationListener
    {
        [Import]
        public IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; set; }

        [Import]
        public SignatureHelpManagerProvider SignatureHelpManagerProvider { get; set; }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            var textView = EditorAdaptersFactoryService.GetWpfTextView(textViewAdapter);
            var signatureHelpManager = SignatureHelpManagerProvider.GetSignatureHelpManager(textView);
            textView.Properties.GetOrCreateSingletonProperty(() => new SignatureHelpCommandHandlerParamInfo(textViewAdapter, textView, signatureHelpManager));
            textView.Properties.GetOrCreateSingletonProperty(() => new SignatureHelpCommandHandlerTypeChar(textViewAdapter, textView, signatureHelpManager));
        }
    }
}