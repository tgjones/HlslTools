using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.TextManager.Interop;
using ShaderTools.VisualStudio.Core;
using ShaderTools.VisualStudio.Core.Navigation;
using ShaderTools.VisualStudio.Core.Util.Extensions;

namespace ShaderTools.VisualStudio.Hlsl.Navigation
{
    internal sealed class CodeWindowManager : CodeWindowManagerBase
    {
        public CodeWindowManager(LanguagePackageBase languagePackage, IVsCodeWindow codeWindow, SVsServiceProvider serviceProvider)
            : base(languagePackage, codeWindow, serviceProvider)
        {
        }

        protected override IVsDropdownBarClient CreateDropdownBarClient()
        {
            var componentModel = ServiceProvider.GetComponentModel();
            var editorAdaptersFactory = componentModel.DefaultExportProvider.GetExportedValueOrDefault<IVsEditorAdaptersFactoryService>();
            var bufferGraphFactoryService = componentModel.DefaultExportProvider.GetExportedValue<IBufferGraphFactoryService>();

            var textView = editorAdaptersFactory.GetWpfTextView(CodeWindow.GetPrimaryView());

            var editorNavigationSourceProvider = componentModel.DefaultExportProvider.GetExportedValueOrDefault<EditorNavigationSourceProvider>();
            var editorNavigationSource = editorNavigationSourceProvider.TryCreateEditorNavigationSource(textView.TextBuffer);

            return new EditorNavigationDropdownBarClient(CodeWindow, editorAdaptersFactory, editorNavigationSource, bufferGraphFactoryService);
        }
    }
}