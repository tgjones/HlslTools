using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using ShaderTools.CodeAnalysis.GoToDefinition;
using ShaderTools.CodeAnalysis.Shared.Extensions;

namespace ShaderTools.LanguageServer.Handlers
{
    internal sealed class DefinitionHandler : IDefinitionHandler
    {
        private readonly LanguageServerWorkspace _workspace;
        private readonly DefinitionRegistrationOptions _registrationOptions;

        public DefinitionHandler(LanguageServerWorkspace workspace, DocumentSelector documentSelector)
        {
            _workspace = workspace;
            _registrationOptions = new DefinitionRegistrationOptions
            {
                DocumentSelector = documentSelector
            };
        }

        public async Task<LocationOrLocationLinks> Handle(DefinitionParams request, CancellationToken token)
        {
            var (document, position) = _workspace.GetLogicalDocument(request);

            var goToDefinitionService = document.GetLanguageService<IGoToDefinitionService>();
            var definitions = await goToDefinitionService.FindDefinitionsAsync(document, position, token);

            // TODO: Handle spans within embedded HLSL blocks; the TextSpan is currently relative to the start of the embedded block.

            var locations = new List<LocationOrLocationLink>();

            foreach (var definition in definitions)
            {
                var documentSpan = definition.SourceSpans[0];
                var sourceSpan = documentSpan.SourceSpan;
                locations.Add(new Location
                {
                    Uri = Helpers.ToUri(sourceSpan.File.FilePath),
                    Range = sourceSpan.IsInRootFile
                        ? Helpers.ToRange(document.SourceText, sourceSpan.Span)
                        : Helpers.ToRange(sourceSpan.File.Text, sourceSpan.Span)
                });
            }

            return locations;
        }

        DefinitionRegistrationOptions IRegistration<DefinitionRegistrationOptions>.GetRegistrationOptions()
        {
            return _registrationOptions;
        }

        void ICapability<DefinitionCapability>.SetCapability(DefinitionCapability capability) { }
    }
}
