using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using ShaderTools.CodeAnalysis.NavigateTo;

namespace ShaderTools.LanguageServer.Handlers
{
    internal sealed class DocumentSymbolsHandler : DocumentSymbolHandlerBase
    {
        private readonly LanguageServerWorkspace _workspace;
        private readonly DocumentSelector _documentSelector;

        public DocumentSymbolsHandler(LanguageServerWorkspace workspace, DocumentSelector documentSelector)
        {
            _workspace = workspace;
            _documentSelector = documentSelector;
        }

        protected override DocumentSymbolRegistrationOptions CreateRegistrationOptions(DocumentSymbolCapability capability, ClientCapabilities clientCapabilities)
        {
            return new DocumentSymbolRegistrationOptions
            {
                DocumentSelector = _documentSelector,
            };
        }

        public override async Task<SymbolInformationOrDocumentSymbolContainer> Handle(DocumentSymbolParams request, CancellationToken token)
        {
            var document = _workspace.GetDocument(request.TextDocument.Uri);

            var searchService = _workspace.Services.GetService<INavigateToSearchService>();

            var symbols = ImmutableArray.CreateBuilder<SymbolInformation>();

            await Helpers.FindSymbolsInDocument(searchService, document, string.Empty, token, symbols);

            var symbolsResult = ImmutableArray.CreateRange(
                symbols.ToImmutable(), 
                x => new SymbolInformationOrDocumentSymbol(x));

            return new SymbolInformationOrDocumentSymbolContainer(symbolsResult);
        }
    }
}
