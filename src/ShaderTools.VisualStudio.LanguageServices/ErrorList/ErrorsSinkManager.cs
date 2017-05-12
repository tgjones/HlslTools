using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell.TableManager;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Diagnostics;

namespace ShaderTools.VisualStudio.LanguageServices.ErrorList
{
    internal sealed class ErrorsSinkManager : IDisposable
    {
        private readonly ITableDataSink _sink;
        private readonly Workspace _workspace;
        private readonly IDiagnosticService _diagnosticService;

        private readonly Dictionary<DocumentId, ErrorsSnapshotFactory> _factories;

        public ErrorsSinkManager(ITableDataSink sink, Workspace workspace)
        {
            _sink = sink;
            _workspace = workspace;

            _diagnosticService = workspace.Services.GetRequiredService<IDiagnosticService>();

            _factories = new Dictionary<DocumentId, ErrorsSnapshotFactory>();

            // Add any documents that were opened before we were created.
            foreach (var documentId in _workspace.CurrentDocuments.DocumentIds)
                AddDocument(documentId);

            workspace.DocumentOpened += OnDocumentOpened;
            workspace.DocumentChanged += OnDocumentChanged;
            workspace.DocumentClosed += OnDocumentClosed;
        }

        private void AddDocument(DocumentId documentId)
        {
            var factory = new ErrorsSnapshotFactory(_diagnosticService, documentId);

            _factories.Add(documentId, factory);
            _sink.AddFactory(factory);

            OnDocumentChanged(documentId);
        }

        private void OnDocumentOpened(object sender, DocumentEventArgs e)
        {
            AddDocument(e.Document.Id);
        }

        private void OnDocumentChanged(object sender, DocumentEventArgs e)
        {
            OnDocumentChanged(e.Document.Id);
        }

        private void OnDocumentChanged(DocumentId documentId)
        {
            if (_factories.TryGetValue(documentId, out var factory))
            {
                factory.OnDocumentChanged(() => _sink.FactorySnapshotChanged(factory));
            }
        }

        private void OnDocumentClosed(object sender, DocumentEventArgs e)
        {
            if (_factories.TryGetValue(e.Document.Id, out var factory))
            {
                _sink.RemoveFactory(factory);
                _factories.Remove(e.Document.Id);
            }
        }

        public void Dispose()
        {
            _workspace.DocumentClosed -= OnDocumentClosed;
            _workspace.DocumentChanged -= OnDocumentChanged;
            _workspace.DocumentOpened -= OnDocumentOpened;

            foreach (var factory in _factories.Values)
                factory.Dispose();

            _factories.Clear();
        }
    }
}
