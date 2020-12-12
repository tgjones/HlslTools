using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using ShaderTools.LanguageServer.Services.SignatureHelp;

namespace ShaderTools.LanguageServer.Handlers
{
    internal sealed class SignatureHelpHandler : ISignatureHelpHandler
    {
        private readonly LanguageServerWorkspace _workspace;
        private readonly SignatureHelpRegistrationOptions _registrationOptions;

        public SignatureHelpHandler(LanguageServerWorkspace workspace, DocumentSelector documentSelector)
        {
            _workspace = workspace;
            _registrationOptions = new SignatureHelpRegistrationOptions
            {
                DocumentSelector = documentSelector
            }; ;
        }

        public Task<SignatureHelp> Handle(SignatureHelpParams request, CancellationToken token)
        {
            var (document, position) = _workspace.GetLogicalDocument(request);

            var signatureHelpService = _workspace.Services.GetService<SignatureHelpService>();

            return signatureHelpService.GetResultAsync(document, position, token);
        }

        SignatureHelpRegistrationOptions IRegistration<SignatureHelpRegistrationOptions>.GetRegistrationOptions() => _registrationOptions;

        void ICapability<SignatureHelpCapability>.SetCapability(SignatureHelpCapability capability) { }
    }
}
