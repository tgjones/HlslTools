using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host;

namespace ShaderTools.CodeAnalysis.Diagnostics
{
    internal interface IDiagnosticService : IWorkspaceService
    {
        event EventHandler<DiagnosticsUpdatedEventArgs> DiagnosticsUpdated;

        Task<ImmutableArray<MappedDiagnostic>> GetDiagnosticsAsync(DocumentId documentId, CancellationToken cancellationToken);
    }
}
