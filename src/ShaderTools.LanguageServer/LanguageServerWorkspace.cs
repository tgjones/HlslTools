using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.LanguageServer
{
    internal sealed class LanguageServerWorkspace : Workspace
    {
        private readonly IMefHostExportProvider _hostServices;

        public LanguageServerWorkspace(MefHostServices hostServices)
            : base(hostServices)
        {
            _hostServices = hostServices;
        }

        public Document GetDocument(DocumentUri uri)
        {
            return CurrentDocuments.GetDocumentWithFilePath(Helpers.FromUri(uri));
        }

        public (Document logicalDocument, int position) GetLogicalDocument(TextDocumentPositionParams textDocumentPositionParams)
        {
            var document = GetDocument(textDocumentPositionParams.TextDocument.Uri);

            var documentPosition = document.SourceText.Lines.GetPosition(new LinePosition(
                textDocumentPositionParams.Position.Line,
                textDocumentPositionParams.Position.Character));

            return (document, documentPosition);
        }

        public Document OpenDocument(DocumentUri uri, string text, string languageId)
        {
            var filePath = Helpers.FromUri(uri);

            var documentId = DocumentId.CreateNewId(filePath);
            var sourceText = SourceText.From(text);

            var document = CreateDocument(documentId, languageId, new SourceFile(sourceText, filePath));
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

        public T GetGlobalService<T>()
            where T : class
        {
            return _hostServices.GetExports<T>().FirstOrDefault()?.Value;
        }
    }
}
