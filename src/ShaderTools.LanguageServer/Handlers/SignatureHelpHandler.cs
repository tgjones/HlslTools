using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using ShaderTools.LanguageServer.Services.SignatureHelp;

namespace ShaderTools.LanguageServer.Handlers
{
    internal sealed class SignatureHelpHandler : ISignatureHelpHandler
    {
        private readonly LanguageServerWorkspace _workspace;
        private readonly TextDocumentRegistrationOptions _registrationOptions;

        public SignatureHelpHandler(LanguageServerWorkspace workspace, TextDocumentRegistrationOptions registrationOptions)
        {
            _workspace = workspace;
            _registrationOptions = registrationOptions;
        }

        public SignatureHelpRegistrationOptions GetRegistrationOptions()
        {
            return new SignatureHelpRegistrationOptions
            {
                DocumentSelector = _registrationOptions.DocumentSelector,
                TriggerCharacters = new Container<string>("(")
            };
        }

        public Task<SignatureHelp> Handle(SignatureHelpParams request, CancellationToken token)
        {
            var (document, position) = _workspace.GetLogicalDocument(request);

            var signatureHelpService = _workspace.Services.GetService<SignatureHelpService>();

            return signatureHelpService.GetResultAsync(document, position, token);
        }

        public void SetCapability(SignatureHelpCapability capability) { }
    }
}
