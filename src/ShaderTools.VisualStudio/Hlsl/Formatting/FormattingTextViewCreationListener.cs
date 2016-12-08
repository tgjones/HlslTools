using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.VisualStudio.Hlsl.Options;

namespace ShaderTools.VisualStudio.Hlsl.Formatting
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType(HlslConstants.ContentTypeName)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class FormattingTextViewCreationListener : IVsTextViewCreationListener
    {
        [Import]
        public IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; set; }

        [Import]
        public IHlslOptionsService OptionsService { get; set; }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            var textView = EditorAdaptersFactoryService.GetWpfTextView(textViewAdapter);
            textView.Properties.GetOrCreateSingletonProperty(() => new FormatCommandTarget(textViewAdapter, textView, OptionsService));
        }
    }
}