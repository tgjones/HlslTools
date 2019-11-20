using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace ShaderTools.LanguageServer.Handlers
{
    internal sealed class TextDocumentSyncHandler : ITextDocumentSyncHandler 
    {
        private readonly LanguageServerWorkspace _workspace;
        private readonly TextDocumentRegistrationOptions _registrationOptions;

        public TextDocumentSyncKind Change { get; } = TextDocumentSyncKind.Incremental;

        public TextDocumentSyncHandler(LanguageServerWorkspace workspace, TextDocumentRegistrationOptions registrationOptions)
        {
            _workspace = workspace;
            _registrationOptions = registrationOptions;
        }

        public TextDocumentChangeRegistrationOptions GetRegistrationOptions() => new TextDocumentChangeRegistrationOptions
        {
            DocumentSelector = _registrationOptions.DocumentSelector,
            SyncKind = TextDocumentSyncKind.Incremental
        };

        public TextDocumentAttributes GetTextDocumentAttributes(Uri uri)
        {
            return new TextDocumentAttributes(uri, string.Empty);
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

        public void SetCapability(SynchronizationCapability capability) { }

        TextDocumentRegistrationOptions IRegistration<TextDocumentRegistrationOptions>.GetRegistrationOptions() => _registrationOptions;

        TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions>.GetRegistrationOptions() => new TextDocumentSaveRegistrationOptions();
    }
}
