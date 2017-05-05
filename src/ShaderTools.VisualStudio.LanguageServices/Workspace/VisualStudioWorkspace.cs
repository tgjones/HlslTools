using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.EditorServices.Host;
using ShaderTools.CodeAnalysis.Text;
using System.ComponentModel.Composition.Hosting;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;

namespace ShaderTools.VisualStudio.LanguageServices
{
    [Export(typeof(VisualStudioWorkspace))]
    internal sealed class VisualStudioWorkspace : Workspace
    {
        private readonly BackgroundParser _backgroundParser;

        [ImportingConstructor]
        public VisualStudioWorkspace(SVsServiceProvider serviceProvider)
            : base(MefV1HostServices.Create(GetExportProvider(serviceProvider)))
        {
            _backgroundParser = new BackgroundParser(this);
            _backgroundParser.Start();
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

        internal void OnDocumentOpened(ITextBuffer textBuffer)
        {
            if (!textBuffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument textDocument))
                return;

            var documentId = new DocumentId(textDocument.FilePath);

            var textContainer = textBuffer.AsTextContainer();

            var document = CreateDocument(documentId, textBuffer.ContentType.TypeName, textContainer.CurrentText);

            OnDocumentOpened(document, textContainer);
        }

        internal void OnDocumentClosed(ITextBuffer textBuffer)
        {
            if (!textBuffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument textDocument))
                throw new InvalidOperationException(); // TODO: Store weak link from text buffer to text document.

            var documentId = new DocumentId(textDocument.FilePath);

            // TODO: Check whether there are any other views associated with this buffer.
            OnDocumentClosed(documentId);
        }
    }
}
