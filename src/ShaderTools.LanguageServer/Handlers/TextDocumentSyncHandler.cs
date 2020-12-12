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
    internal sealed class TextDocumentSyncHandler : ITextDocumentSyncHandler
    {
        private readonly LanguageServerWorkspace _workspace;
        private readonly TextDocumentChangeRegistrationOptions _changeRegistrationOptions;

        public TextDocumentSyncHandler(LanguageServerWorkspace workspace, DocumentSelector documentSelector)
        {
            _workspace = workspace;
            _changeRegistrationOptions = new TextDocumentChangeRegistrationOptions
            {
                DocumentSelector = documentSelector,
                SyncKind = TextDocumentSyncKind.Incremental,
            };
        }

        public TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
        {
            var document = _workspace.GetDocument(uri);

            return new TextDocumentAttributes(
                uri,
                Helpers.ToLspLanguage(document.Language));
        }

        public Task<Unit> Handle(DidChangeTextDocumentParams notification, CancellationToken cancellationToken)
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

        public Task<Unit> Handle(DidOpenTextDocumentParams notification, CancellationToken cancellationToken)
        {
            _workspace.OpenDocument(
                notification.TextDocument.Uri,
                notification.TextDocument.Text,
                notification.TextDocument.LanguageId);

            return Unit.Task;
        }

        public Task<Unit> Handle(DidCloseTextDocumentParams notification, CancellationToken cancellationToken)
        {
            var document = _workspace.GetDocument(notification.TextDocument.Uri);

            if (document != null)
            {
                _workspace.CloseDocument(document.Id);
            }

            return Unit.Task;
        }

        public Task<Unit> Handle(DidSaveTextDocumentParams notification, CancellationToken cancellationToken) => Unit.Task;

        TextDocumentChangeRegistrationOptions IRegistration<TextDocumentChangeRegistrationOptions>.GetRegistrationOptions() => _changeRegistrationOptions;

        TextDocumentRegistrationOptions IRegistration<TextDocumentRegistrationOptions>.GetRegistrationOptions()
        {
            return _changeRegistrationOptions;
        }

        void ICapability<SynchronizationCapability>.SetCapability(SynchronizationCapability capability) { }

        TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions>.GetRegistrationOptions() => null;
    }
}
