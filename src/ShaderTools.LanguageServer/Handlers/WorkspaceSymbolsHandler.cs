using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using ShaderTools.CodeAnalysis.NavigateTo;

namespace ShaderTools.LanguageServer.Handlers
{
    internal sealed class WorkspaceSymbolsHandler : IWorkspaceSymbolsHandler
    {
        private readonly LanguageServerWorkspace _workspace;

        public WorkspaceSymbolsHandler(LanguageServerWorkspace workspace)
        {
            _workspace = workspace;
        }

        public async Task<SymbolInformationContainer> Handle(WorkspaceSymbolParams request, CancellationToken token)
        {
            var searchService = _workspace.Services.GetService<INavigateToSearchService>();

            var symbols = ImmutableArray.CreateBuilder<SymbolInformation>();

            foreach (var document in _workspace.CurrentDocuments.Documents)
            {
                await Helpers.FindSymbolsInDocument(searchService, document, request.Query, token, symbols);
            }

            return new SymbolInformationContainer(symbols);
        }

        public void SetCapability(WorkspaceSymbolCapability capability) { }

        object IRegistration<object>.GetRegistrationOptions()
        {
            return null;
        }
    }
}
