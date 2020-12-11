using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace ShaderTools.LanguageServer.Handlers
{
    internal sealed class TextDocumentSyncHandler : TextDocumentSyncHandlerBase 
    {
        private readonly LanguageServerWorkspace _workspace;
        private readonly DocumentSelector _documentSelector;

        public TextDocumentSyncHandler(LanguageServerWorkspace workspace, DocumentSelector documentSelector)
        {
            _workspace = workspace;
            _documentSelector = documentSelector;
        }

        protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(SynchronizationCapability capability, ClientCapabilities clientCapabilities)
        {
            return new TextDocumentSyncRegistrationOptions(TextDocumentSyncKind.Incremental)
            {
                DocumentSelector = _documentSelector,
                Change = TextDocumentSyncKind.Incremental,
            };
        }

        public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
        {
            var document = _workspace.GetDocument(uri);

            return new TextDocumentAttributes(
                uri,
                Helpers.ToLspLanguage(document.Language));
        }

        public override Task<Unit> Handle(DidChangeTextDocumentParams notification, CancellationToken cancellationToken)
        {
            var document = _workspace.GetDocument(notification.TextDocument.Uri);

            if (document == null)
            {
                return Unit.Task;
            }

            _workspace.UpdateDocument(
                document,
                notification.ContentChanges.Select(x =>
                    Helpers.ToTextChange(
                        document,
                        x.Range,
                        x.Text)));

            return Unit.Task;
        }

        public override Task<Unit> Handle(DidOpenTextDocumentParams notification, CancellationToken cancellationToken)
        {
            _workspace.OpenDocument(
                notification.TextDocument.Uri,
                notification.TextDocument.Text,
                notification.TextDocument.LanguageId);

            return Unit.Task;
        }

        public override Task<Unit> Handle(DidCloseTextDocumentParams notification, CancellationToken cancellationToken)
        {
            var document = _workspace.GetDocument(notification.TextDocument.Uri);

            if (document != null)
            {
                _workspace.CloseDocument(document.Id);
            }

            return Unit.Task;
        }

        public override Task<Unit> Handle(DidSaveTextDocumentParams notification, CancellationToken cancellationToken) => Unit.Task;
    }
}
