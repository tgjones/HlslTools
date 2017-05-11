using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.VisualStudio.LanguageServices
{
    [Export(typeof(VisualStudioWorkspace))]
    internal sealed class VisualStudioWorkspace : Workspace
    {
        private readonly ITextDocumentFactoryService _textDocumentFactoryService;
        private readonly BackgroundParser _backgroundParser;
        private readonly ConditionalWeakTable<ITextBuffer, ITextDocument> _textBufferToDocumentMap;
        private readonly ConditionalWeakTable<ITextBuffer, List<ITextView>> _textBufferToViewsMap;

        [ImportingConstructor]
        public VisualStudioWorkspace(
            SVsServiceProvider serviceProvider,
            ITextDocumentFactoryService textDocumentFactoryService)
            : base(MefV1HostServices.Create(GetExportProvider(serviceProvider)))
        {
            PrimaryWorkspace.Register(this);

            _textDocumentFactoryService = textDocumentFactoryService;

            _backgroundParser = new BackgroundParser(this);
            _backgroundParser.Start();

            _textBufferToDocumentMap = new ConditionalWeakTable<ITextBuffer, ITextDocument>();
            _textBufferToViewsMap = new ConditionalWeakTable<ITextBuffer, List<ITextView>>();
        }

        private static ExportProvider GetExportProvider(SVsServiceProvider serviceProvider)
        {
            var componentModel = (IComponentModel) serviceProvider.GetService(typeof(SComponentModel));
            return componentModel.DefaultExportProvider;
        }

        protected override void OnDocumentTextChanged(Document document)
        {
            if (_backgroundParser != null)
            {
                _backgroundParser.Parse(document);
            }
        }

        protected override void OnDocumentClosing(DocumentId documentId)
        {
            if (_backgroundParser != null)
            {
                _backgroundParser.CancelParse(documentId);
            }
        }

        internal void OnTextViewCreated(ITextView textView)
        {
            var textBuffer = textView.TextBuffer;

            // Add this ITextView to the list of known views for this buffer.
            _textBufferToViewsMap.GetOrCreateValue(textBuffer).Add(textView);

            // Do we already know about this text buffer?
            if (_textBufferToDocumentMap.TryGetValue(textBuffer, out var _))
            {
                return;
            }

            if (!_textDocumentFactoryService.TryGetTextDocument(textBuffer, out var textDocument))
            {
                // Don't know why this would ever happen.
                return;
            }

            _textBufferToDocumentMap.Add(textBuffer, textDocument);

            // TODO: hookup textDocument.FileActionOccurred for renames.

            var documentId = new DocumentId(textDocument.FilePath);

            var textContainer = textBuffer.AsTextContainer();

            var document = CreateDocument(documentId, textBuffer.ContentType.TypeName, textContainer.CurrentText);

            OnDocumentOpened(document);
        }

        internal void OnTextViewClosed(ITextView textView)
        {
            var textBuffer = textView.TextBuffer;

            // Remove text view from the list of known text views for this text buffer.
            var textViews = _textBufferToViewsMap.GetOrCreateValue(textBuffer);
            textViews.Remove(textView);

            // Only close document if this is the last view referencing the text buffer.
            if (textViews.Count > 0)
            {
                return;
            }

            if (!_textBufferToDocumentMap.TryGetValue(textBuffer, out var textDocument))
            {
                throw new InvalidOperationException();
            }

            var documentId = new DocumentId(textDocument.FilePath);

            OnDocumentClosed(documentId);

            _textBufferToViewsMap.Remove(textBuffer);
            _textBufferToDocumentMap.Remove(textBuffer);
        }
    }
}
