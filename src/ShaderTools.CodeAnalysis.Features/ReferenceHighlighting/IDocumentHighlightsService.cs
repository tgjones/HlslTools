using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host;

namespace ShaderTools.CodeAnalysis.ReferenceHighlighting
{
    internal interface IDocumentHighlightsService : IWorkspaceService
    {
        Task<ImmutableArray<DocumentHighlights>> GetDocumentHighlightsAsync(
            Document document, int position, IImmutableSet<Document> documentsToSearch, CancellationToken cancellationToken);
    }
}
