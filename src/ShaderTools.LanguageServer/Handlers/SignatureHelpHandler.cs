using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using ShaderTools.LanguageServer.Services.SignatureHelp;

namespace ShaderTools.LanguageServer.Handlers
{
    internal sealed class SignatureHelpHandler : SignatureHelpHandlerBase
    {
        private readonly LanguageServerWorkspace _workspace;
        private readonly DocumentSelector _documentSelector;

        public SignatureHelpHandler(LanguageServerWorkspace workspace, DocumentSelector documentSelector)
        {
            _workspace = workspace;
            _documentSelector = documentSelector;
        }

        protected override SignatureHelpRegistrationOptions CreateRegistrationOptions(SignatureHelpCapability capability, ClientCapabilities clientCapabilities)
        {
            return new SignatureHelpRegistrationOptions
            {
                DocumentSelector = _documentSelector
            };
        }

        public override Task<SignatureHelp> Handle(SignatureHelpParams request, CancellationToken token)
        {
            var (document, position) = _workspace.GetLogicalDocument(request);

            var signatureHelpService = _workspace.Services.GetService<SignatureHelpService>();

            return signatureHelpService.GetResultAsync(document, position, token);
        }
    }
}
