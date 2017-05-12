using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.TableManager;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Editor.Shared.Threading;
using ShaderTools.CodeAnalysis.Shared.TestHooks;

namespace ShaderTools.VisualStudio.LanguageServices.ErrorList
{
    internal sealed class ErrorsSnapshotFactory : TableEntriesSnapshotFactoryBase
    {
        private readonly IDiagnosticService _diagnosticService;
        private readonly DocumentId _documentId;

        private readonly AsynchronousSerialWorkQueue _workQueue;

        private ErrorsSnapshot _currentSnapshot;

        public override int CurrentVersionNumber => _currentSnapshot.VersionNumber;

        public ErrorsSnapshotFactory(IDiagnosticService diagnosticService, DocumentId documentId)
        {
            _diagnosticService = diagnosticService;
            _documentId = documentId;

            _workQueue = new AsynchronousSerialWorkQueue(new AsynchronousOperationListener());

            _currentSnapshot = new ErrorsSnapshot(ImmutableArray<MappedDiagnostic>.Empty, 0);
        }

        public void OnDocumentChanged(Action onCompleted)
        {
            _workQueue.CancelCurrentWork();

            var cancellationToken = _workQueue.CancellationToken;

            _workQueue.EnqueueBackgroundTask(
                async ct =>
                {
                    await UpdateCurrentSnapshotAsync(ct);
                    onCompleted();
                }, 
                "BuildDiagnostics", 
                1000, 
                cancellationToken);
        }

        private async Task UpdateCurrentSnapshotAsync(CancellationToken cancellationToken)
        {
            var diagnostics = await _diagnosticService
                .GetDiagnosticsAsync(_documentId, cancellationToken)
                .ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var snapshot = new ErrorsSnapshot(
                diagnostics,
                _currentSnapshot.VersionNumber + 1);

            _currentSnapshot = snapshot;
        }

        public override ITableEntriesSnapshot GetCurrentSnapshot()
        {
            return _currentSnapshot;
        }

        public override ITableEntriesSnapshot GetSnapshot(int versionNumber)
        {
            var snapshot = _currentSnapshot;

            return versionNumber == snapshot.VersionNumber
                ? snapshot
                : null;
        }

        public override void Dispose()
        {
            _workQueue.CancelCurrentWork();

            base.Dispose();
        }
    }
}
