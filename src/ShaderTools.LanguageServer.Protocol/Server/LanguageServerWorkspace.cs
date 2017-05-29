using System.Collections.Generic;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.LanguageServer.Protocol.Server
{
    public sealed class LanguageServerWorkspace : Workspace
    {
        public LanguageServerWorkspace(MefHostServices hostServices)
            : base(hostServices)
        {
        }

        public Document OpenDocument(DocumentId documentId, SourceText sourceText, string languageName)
        {
            var document = CreateDocument(documentId, languageName, sourceText, sourceText.FilePath);
            OnDocumentOpened(document);
            return document;
        }

        public Document UpdateDocument(Document document, IEnumerable<TextChange> changes)
        {
            var newText = document.SourceText.WithChanges(changes);
            OnDocumentTextChanged(document.Id, newText);
            return CurrentDocuments.GetDocument(document.Id);
        }

        public void CloseDocument(DocumentId documentId)
        {
            OnDocumentClosed(documentId);
        }
    }
}
