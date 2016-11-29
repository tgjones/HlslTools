using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace ShaderTools.VisualStudio.Hlsl.IntelliSense.Completion
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType(HlslConstants.ContentTypeName)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class CompletionCommandHandlerProvider : IVsTextViewCreationListener
    {
        [Import]
        public IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; set; }

        [Import]
        public CompletionModelManagerProvider CompletionModelManagerProvider { get; set; }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            var textView = EditorAdaptersFactoryService.GetWpfTextView(textViewAdapter);
            var completionModelManager = CompletionModelManagerProvider.GetCompletionModel(textView);
            textView.Properties.GetOrCreateSingletonProperty(() => new CompletionCommandHandlerTriggers(textViewAdapter, textView, completionModelManager));
            textView.Properties.GetOrCreateSingletonProperty(() => new CompletionCommandHandler(textViewAdapter, textView, completionModelManager));
        }
    }
}