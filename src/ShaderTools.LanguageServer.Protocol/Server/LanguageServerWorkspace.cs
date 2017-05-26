using System.Collections.Generic;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.LanguageServer.Protocol.Server
{
    public sealed class LanguageServerWorkspace : Workspace
    {
        public LanguageServerWorkspace()
            : base(MefHostServices.DefaultHost)
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
            throw new System.NotImplementedException();
            //return OnDocumentTextChanged(document.Id, newText);
        }

        public void CloseDocument(DocumentId documentId)
        {
            OnDocumentClosed(documentId);
        }
    }
}
