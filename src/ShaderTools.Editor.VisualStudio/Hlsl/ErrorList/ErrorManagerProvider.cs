using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.Editor.VisualStudio.Hlsl.Options;
using ShaderTools.Editor.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.Editor.VisualStudio.Hlsl.ErrorList
{
    //[Export(typeof(IWpfTextViewCreationListener))]
    [ContentType(HlslConstants.ContentTypeName)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class ErrorManagerProvider : IWpfTextViewCreationListener
    {
        [Import]
        public IHlslOptionsService OptionsService { get; set; }

        [Import]
        public SVsServiceProvider ServiceProvider { get; set; }

        [Import]
        public ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        public void TextViewCreated(IWpfTextView textView)
        {
            textView.Properties.GetOrCreateSingletonProperty(() => new SyntaxErrorManager(textView.TextBuffer.GetBackgroundParser(), textView, OptionsService, ServiceProvider, TextDocumentFactoryService));
            textView.Properties.GetOrCreateSingletonProperty(() => new SemanticErrorManager(textView.TextBuffer.GetBackgroundParser(), textView, OptionsService, ServiceProvider, TextDocumentFactoryService));
        }
    }
}