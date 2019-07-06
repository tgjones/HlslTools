using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Diagnostics
{
    internal sealed class DiagnosticService : IDiagnosticService
    {
        public event EventHandler<DiagnosticsUpdatedEventArgs> DiagnosticsUpdated;

        private readonly Workspace _workspace;

        public DiagnosticService(Workspace workspace)
        {
            _workspace = workspace;

            workspace.DocumentOpened += OnDocumentEvent;
            workspace.DocumentClosed += OnDocumentEvent;
            workspace.DocumentChanged += OnDocumentEvent;
        }

        private void OnDocumentEvent(object sender, DocumentEventArgs e)
        {
            DiagnosticsUpdated?.Invoke(this, new DiagnosticsUpdatedEventArgs(e.Document));
        }

        public async Task<ImmutableArray<MappedDiagnostic>> GetDiagnosticsAsync(DocumentId documentId, CancellationToken cancellationToken)
        {
            var document = _workspace.CurrentDocuments.GetDocument(documentId);

            if (document == null)
                return ImmutableArray<MappedDiagnostic>.Empty;

            var options = await document.GetOptionsAsync(cancellationToken).ConfigureAwait(false);
            if (!options.GetOption(DiagnosticsOptions.EnableErrorReporting))
                return ImmutableArray<MappedDiagnostic>.Empty;

            var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            if (syntaxTree == null)
                return ImmutableArray<MappedDiagnostic>.Empty;

            var result = ImmutableArray.CreateBuilder<MappedDiagnostic>();

            foreach (var diagnostic in syntaxTree.GetDiagnostics())
                result.Add(MapDiagnostic(syntaxTree, diagnostic, DiagnosticSource.SyntaxParsing));

            cancellationToken.ThrowIfCancellationRequested();

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (semanticModel != null)
                foreach (var diagnostic in semanticModel.GetDiagnostics())
                    result.Add(MapDiagnostic(syntaxTree, diagnostic, DiagnosticSource.SemanticAnalysis));

            return result.ToImmutable();
        }

        private static MappedDiagnostic MapDiagnostic(SyntaxTreeBase syntaxTree, Diagnostic diagnostic, DiagnosticSource source)
        {
            var fileSpan = syntaxTree.GetSourceFileSpan(diagnostic.SourceRange);
            return new MappedDiagnostic(diagnostic, source, fileSpan);
        }
    }
}