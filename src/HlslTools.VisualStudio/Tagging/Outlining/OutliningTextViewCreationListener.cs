using System.ComponentModel.Composition;
using HlslTools.VisualStudio.Options;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace HlslTools.VisualStudio.Tagging.Outlining
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType(HlslConstants.ContentTypeName)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class OutliningTextViewCreationListener : IVsTextViewCreationListener
    {
        [Import]
        public IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; set; }

        [Import]
        public IOutliningManagerService OutliningManagerService { get; set; }

        [Import]
        public IOptionsService OptionsService { get; set; }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            var textView = EditorAdaptersFactoryService.GetWpfTextView(textViewAdapter);

            var outliningManager = OutliningManagerService.GetOutliningManager(textView);

            if (!OptionsService.AdvancedOptions.EnterOutliningModeWhenFilesOpen)
                outliningManager.Enabled = false;

            textView.Properties.GetOrCreateSingletonProperty(() => new OutliningCommandTarget(textViewAdapter, textView, OutliningManagerService));
        }
    }
}