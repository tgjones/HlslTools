using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace HlslTools.VisualStudio.ErrorList
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType(HlslConstants.ContentTypeName)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class ErrorListManager : IWpfTextViewCreationListener
    {
        private readonly SVsServiceProvider _serviceProvider;
        private readonly ITextDocumentFactoryService _textDocumentFactoryService;

        [ImportingConstructor]
        public ErrorListManager(SVsServiceProvider serviceProvider, ITextDocumentFactoryService textDocumentFactoryService)
        {
            _serviceProvider = serviceProvider;
            _textDocumentFactoryService = textDocumentFactoryService;
        }

        public void TextViewCreated(IWpfTextView textView)
        {
            textView.Closed += OnViewClosed;

            ITextDocument document;
            if (_textDocumentFactoryService.TryGetTextDocument(textView.TextBuffer, out document))
                textView.TextBuffer.Properties.GetOrCreateSingletonProperty(
                    () => new ErrorListHelper(_serviceProvider, textView, document));
        }

        private static void OnViewClosed(object sender, EventArgs e)
        {
            var view = (IWpfTextView)sender;
            view.Closed -= OnViewClosed;

            var errorListHelper = view.TextBuffer.GetErrorListHelper();
            errorListHelper?.Dispose();
        }
    }
}