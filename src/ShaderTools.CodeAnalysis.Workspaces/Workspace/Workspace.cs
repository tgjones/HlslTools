//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using ShaderTools.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis
{
    public abstract partial class Workspace
    {
        private readonly HostWorkspaceServices _services;

        private readonly SemaphoreSlim _serializationLock = new SemaphoreSlim(initialCount: 1);

        private ImmutableDictionary<DocumentId, Document> _openDocuments = ImmutableDictionary<DocumentId, Document>.Empty;
        private ImmutableDictionary<string, ConfigFile> _configFiles = ImmutableDictionary<string, ConfigFile>.Empty;

        private readonly Dictionary<DocumentId, TextTracker> _textTrackers = new Dictionary<DocumentId, TextTracker>();

        public event EventHandler<DocumentEventArgs> DocumentOpened;
        public event EventHandler<DocumentEventArgs> DocumentClosed;

        public HostWorkspaceServices Services => _services;

        protected Workspace(HostServices host)
        {
            _services = host.CreateWorkspaceServices(this);

            _openDocuments = ImmutableDictionary<DocumentId, Document>.Empty;
        }

        public IEnumerable<Document> OpenDocuments
        {
            get
            {
                var latestDocuments = Volatile.Read(ref _openDocuments);
                return latestDocuments.Values;
            }
        }

        public Document GetDocument(DocumentId documentId)
        {
            var latestDocuments = Volatile.Read(ref _openDocuments);
            if (latestDocuments.TryGetValue(documentId, out var document))
                return document;
            return null;
        }

        protected Document CreateDocument(DocumentId documentId, string languageName, SourceText sourceText)
        {
            var languageServices = _services.GetLanguageServices(languageName);
            return new Document(languageServices, documentId, sourceText);
        }

        protected void OnDocumentOpened(Document document)
        {
            ImmutableInterlocked.AddOrUpdate(ref _openDocuments, document.Id, document, (k, v) => document);

            // SignupForTextChanges(documentId, textContainer, isCurrentContext, (w, id, text, mode) => w.OnDocumentTextChanged(id, text, mode));

            OnDocumentTextChanged(document);

            DocumentOpened?.Invoke(this, new DocumentEventArgs(document));
        }

        protected void OnDocumentClosed(DocumentId documentId)
        {
            OnDocumentClosing(documentId);

            ImmutableInterlocked.TryRemove(ref _openDocuments, documentId, out var document);

            DocumentClosed?.Invoke(this, new DocumentEventArgs(document));
        }

        protected void OnDocumentRenamed(DocumentId oldDocumentId, DocumentId newDocumentId)
        {
            if (!ImmutableInterlocked.TryRemove(ref _openDocuments, oldDocumentId, out var oldDocument))
                return;

            ImmutableInterlocked.TryAdd(ref _openDocuments, newDocumentId, oldDocument.WithId(newDocumentId));
        }

        protected Document OnDocumentTextChanged(Document document, SourceText newText)
        {
            var newDocument = ImmutableInterlocked.AddOrUpdate(
                ref _openDocuments,
                document.Id,
                k => document.WithText(newText),
                (k, v) => v.WithText(newText));

            OnDocumentTextChanged(newDocument);

            return newDocument;
        }

        /// <summary>
        /// Override this method to act immediately when the text of a document has changed, as opposed
        /// to waiting for the corresponding workspace changed event to fire asynchronously.
        /// </summary>
        protected virtual void OnDocumentTextChanged(Document document)
        {
        }

        /// <summary>
        /// Override this method to act immediately when a document is closing, as opposed
        /// to waiting for the corresponding workspace changed event to fire asynchronously.
        /// </summary>
        protected virtual void OnDocumentClosing(DocumentId documentId)
        {
        }

        private void SignupForTextChanges(DocumentId documentId, SourceTextContainer textContainer, bool isCurrentContext, Action<Workspace, DocumentId, SourceText> onChangedHandler)
        {
            var tracker = new TextTracker(this, documentId, textContainer, onChangedHandler);
            _textTrackers.Add(documentId, tracker);
            tracker.Connect();
        }

        // TODO: Refactor this.
        public ConfigFile LoadConfigFile(string directory)
        {
            return ImmutableInterlocked.GetOrAdd(
                ref _configFiles, 
                directory.ToLower(), 
                x => ConfigFileLoader.LoadAndMergeConfigFile(x));
        }
    }
}
