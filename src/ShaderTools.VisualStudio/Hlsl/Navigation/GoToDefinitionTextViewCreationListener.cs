using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.VisualStudio.Hlsl.Navigation.GoToDefinitionProviders;
using ShaderTools.VisualStudio.Hlsl.Text;

namespace ShaderTools.VisualStudio.Hlsl.Navigation
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType(HlslConstants.ContentTypeName)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class GoToDefinitionTextViewCreationListener : IVsTextViewCreationListener
    {
        [Import]
        public IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; set; }

        [Import]
        public GoToDefinitionProviderService GoToDefinitionProviderService { get; set; }

        [Import]
        public SVsServiceProvider ServiceProvider { get; set; }

        [Import]
        public VisualStudioSourceTextFactory SourceTextFactory { get; set; }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            var textView = EditorAdaptersFactoryService.GetWpfTextView(textViewAdapter);

            textView.Properties.GetOrCreateSingletonProperty(() => new GoToDefinitionCommandTarget(textViewAdapter, textView, GoToDefinitionProviderService, ServiceProvider));
            textView.Properties.GetOrCreateSingletonProperty(() => new OpenIncludeFileCommandTarget(textViewAdapter, textView, ServiceProvider, SourceTextFactory));
        }
    }
}