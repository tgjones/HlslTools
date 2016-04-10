using System.ComponentModel.Composition;
using HlslTools.VisualStudio.Options;
using HlslTools.VisualStudio.Parsing;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace HlslTools.VisualStudio.ErrorList
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [Name(nameof(ErrorManagerProvider))]
    [ContentType(HlslConstants.ContentTypeName)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class ErrorManagerProvider : IWpfTextViewCreationListener
    {
        [Import]
        public BackgroundParser BackgroundParser { get; set; }

        [Import]
        public IOptionsService OptionsService { get; set; }

        [Import]
        public SVsServiceProvider ServiceProvider { get; set; }

        [Import]
        public ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        public void TextViewCreated(IWpfTextView textView)
        {
            textView.Properties.GetOrCreateSingletonProperty(() => new SyntaxErrorManager(BackgroundParser, textView, OptionsService, ServiceProvider, TextDocumentFactoryService));
            textView.Properties.GetOrCreateSingletonProperty(() => new SemanticErrorManager(BackgroundParser, textView, OptionsService, ServiceProvider, TextDocumentFactoryService));
        }
    }
}