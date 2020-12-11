using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.ReferenceHighlighting;

namespace ShaderTools.LanguageServer.Handlers
{
    internal sealed class DocumentHighlightHandler : DocumentHighlightHandlerBase
    {
        private readonly LanguageServerWorkspace _workspace;
        private readonly DocumentSelector _documentSelector;

        public DocumentHighlightHandler(LanguageServerWorkspace workspace, DocumentSelector documentSelector)
        {
            _workspace = workspace;
            _documentSelector = documentSelector;
        }

        protected override DocumentHighlightRegistrationOptions CreateRegistrationOptions(DocumentHighlightCapability capability, ClientCapabilities clientCapabilities)
        {
            return new DocumentHighlightRegistrationOptions
            {
                DocumentSelector = _documentSelector
            };
        }

        public override async Task<DocumentHighlightContainer> Handle(DocumentHighlightParams request, CancellationToken token)
        {
            var (document, position) = _workspace.GetLogicalDocument(request);

            var documentHighlightsService = _workspace.Services.GetService<IDocumentHighlightsService>();

            var documentHighlightsList = await documentHighlightsService.GetDocumentHighlightsAsync(
                document, position,
                ImmutableHashSet<Document>.Empty, // TODO
                token);

            var result = new List<DocumentHighlight>();

            foreach (var documentHighlights in documentHighlightsList)
            {
                if (documentHighlights.Document != document)
                {
                    continue;
                }

                foreach (var highlightSpan in documentHighlights.HighlightSpans)
                {
                    result.Add(new DocumentHighlight
                    {
                        Kind = highlightSpan.Kind == HighlightSpanKind.Definition
                            ? DocumentHighlightKind.Write
                            : DocumentHighlightKind.Read,
                        Range = Helpers.ToRange(document.SourceText, highlightSpan.TextSpan)
                    });
                }
            }

            return result;
        }
    }
}
