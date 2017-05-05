using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.LanguageServer.Protocol.Server
{
    public sealed class LanguageServerWorkspace : Workspace
    {
        private readonly string _languageName;

        public LanguageServerWorkspace(string languageName)
            : base(MefHostServices.DefaultHost)
        {
            _languageName = languageName;
        }

        public Document OpenDocument(DocumentId documentId, SourceText sourceText)
        {
            var document = CreateDocument(documentId, _languageName, sourceText);
            OnDocumentOpened(document, sourceText.Container);
            return document;
        }

        public Document UpdateDocument(Document document, TextChange change)
        {
            var newText = document.SourceText.WithChanges(change);
            return OnDocumentTextChanged(document.Id, newText);
        }

        public void CloseDocument(DocumentId documentId)
        {
            OnDocumentClosed(documentId);
        }
    }
}
