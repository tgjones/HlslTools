// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Properties;
using ShaderTools.Utilities.Diagnostics;
using ShaderTools.Utilities.Threading;

namespace ShaderTools.CodeAnalysis
{
    // The parts of a workspace that deal with open documents
    public abstract partial class Workspace
    {
        // open documents
        private readonly HashSet<DocumentId> _openDocuments = new HashSet<DocumentId>();

        // text buffer maps
        /// <summary>
        /// Tracks the document ID in the current context for a source text container for an opened text buffer.
        /// </summary>
        /// <remarks>For each entry in this map, there must be a corresponding entry in <see cref="_bufferToAssociatedDocumentsMap"/> where the document ID in current context is one of associated document IDs.</remarks>
        private readonly Dictionary<SourceTextContainer, DocumentId> _bufferToDocumentInCurrentContextMap = new Dictionary<SourceTextContainer, DocumentId>();

        private readonly Dictionary<DocumentId, TextTracker> _textTrackers = new Dictionary<DocumentId, TextTracker>();

        /// <summary>
        /// Gets the id for the document associated with the given text container in its current context.
        /// Documents are normally associated with a text container when the documents are opened.
        /// </summary>
        public virtual DocumentId GetDocumentIdInCurrentContext(SourceTextContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            using (_stateLock.DisposableWait())
            {
                return GetDocumentIdInCurrentContext_NoLock(container);
            }
        }

        private DocumentId GetDocumentIdInCurrentContext_NoLock(SourceTextContainer container)
        {
            bool foundValue = _bufferToDocumentInCurrentContextMap.TryGetValue(container, out var docId);

            if (foundValue)
            {
                return docId;
            }
            else
            {
                return null;
            }
        }

        private void UpdateCurrentContextMapping_NoLock(SourceTextContainer textContainer, DocumentId id)
        {
            if (!_bufferToDocumentInCurrentContextMap.ContainsKey(textContainer))
            {
                _bufferToDocumentInCurrentContextMap[textContainer] = id;
            }
        }

        protected void CheckDocumentIsClosed(DocumentId documentId)
        {
            if (this.IsDocumentOpen(documentId))
            {
                throw new ArgumentException(
                    string.Format(WorkspacesResources._0_is_still_open,
                    this.GetDocumentName(documentId)));
            }
        }

        /// <summary>
        /// Determines if the document is currently open in the host environment.
        /// </summary>
        public virtual bool IsDocumentOpen(DocumentId documentId)
        {
            using (_stateLock.DisposableWait())
            {
                var openDocuments = this.GetOpenDocuments_NoLock();
                return openDocuments != null && openDocuments.Contains(documentId);
            }
        }

        private ISet<DocumentId> GetOpenDocuments_NoLock()
        {
            _stateLock.AssertHasLock();
            return _openDocuments;
        }

        private void AddToOpenDocumentMap(DocumentId documentId)
        {
            using (_stateLock.DisposableWait())
            {
                var openDocuments = GetOpenDocuments_NoLock();
                openDocuments.Add(documentId);
            }
        }

        protected void OnDocumentOpened(Document document)
        {
            CheckDocumentIsClosed(document.Id);

            using (_serializationLock.DisposableWait())
            {
                var oldDocuments = this.CurrentDocuments;
                var oldDocument = oldDocuments.GetDocument(document.Id);

                // TODO: If document exists but is not opened (i.e. a previously closed #include'd file), then re-use it.

                AddToOpenDocumentMap(document.Id);

                var currentDocuments = oldDocument != null 
                    ? oldDocuments.WithDocumentText(document.Id, document.SourceText)
                    : oldDocuments.AddDocument(document);

                var newSolution = this.SetCurrentDocuments(currentDocuments);
                SignupForTextChanges(document.Id, document.SourceText.Container, (w, id, text) => w.OnDocumentTextChanged(id, text));

                var newDoc = newSolution.GetDocument(document.Id);
                this.OnDocumentTextChanged(newDoc);

                DocumentOpened?.Invoke(this, new DocumentEventArgs(newDoc));
            }

            RegisterText(document.SourceText.Container);
        }

        protected void OnDocumentClosed(DocumentId documentId)
        {
            this.CheckDocumentIsOpen(documentId);

            using (_serializationLock.DisposableWait())
            {
                // forget any open document info
                ForgetAnyOpenDocumentInfo(documentId);

                var oldDocuments = this.CurrentDocuments;
                var oldDocument = oldDocuments.GetDocument(documentId);

                this.OnDocumentClosing(documentId);

                // TODO: If document is closed, but is still #include'd by another document, keep it around.

                var newDocuments = oldDocuments.RemoveDocument(documentId);
                newDocuments = this.SetCurrentDocuments(newDocuments);

                var newDoc = newDocuments.GetDocument(documentId);
                this.OnDocumentTextChanged(newDoc);

                DocumentClosed?.Invoke(this, new DocumentEventArgs(oldDocument));
            }
        }

        private void ForgetAnyOpenDocumentInfo(DocumentId documentId)
        {
            using (_stateLock.DisposableWait())
            {
                this.ClearOpenDocument_NoLock(documentId);
            }
        }

        /// <returns>The DocumentId of the current context document for the buffer that was 
        /// previously attached to the given documentId, if any</returns>
        private void ClearOpenDocument_NoLock(DocumentId documentId)
        {
            _stateLock.AssertHasLock();
            _openDocuments.Remove(documentId);

            // Stop tracking the buffer or update the documentId associated with the buffer.
            if (_textTrackers.TryGetValue(documentId, out var tracker))
            {
                tracker.Disconnect();
                _textTrackers.Remove(documentId);

                UpdateCurrentContextMapping_DocumentClosed_NoLock(tracker.TextContainer, documentId);

                // No documentIds are attached to this buffer, so stop tracking it.
                this.UnregisterText(tracker.TextContainer);
            }
        }

        /// <returns>The DocumentId of the current context document attached to the textContainer, if any.</returns>
        private void UpdateCurrentContextMapping_DocumentClosed_NoLock(SourceTextContainer textContainer, DocumentId closedDocumentId)
        {
            Contract.ThrowIfFalse(_bufferToDocumentInCurrentContextMap.ContainsKey(textContainer));

            // Remove the entry if there are no more documents attached to given textContainer.
            _bufferToDocumentInCurrentContextMap.Remove(textContainer);
        }

        //protected void OnDocumentRenamed(DocumentId oldDocumentId, DocumentId newDocumentId)
        //{
        //    if (!ImmutableInterlocked.TryRemove(ref _openDocuments, oldDocumentId, out var oldDocument))
        //        return;

        //    ImmutableInterlocked.TryAdd(ref _openDocuments, newDocumentId, oldDocument.WithId(newDocumentId));
        //}

        private void SignupForTextChanges(DocumentId documentId, SourceTextContainer textContainer, Action<Workspace, DocumentId, SourceText> onChangedHandler)
        {
            var tracker = new TextTracker(this, documentId, textContainer, onChangedHandler);
            _textTrackers.Add(documentId, tracker);
            UpdateCurrentContextMapping_NoLock(textContainer, documentId);
            tracker.Connect();
        }

        protected void CheckDocumentIsOpen(DocumentId documentId)
        {
            if (!this.IsDocumentOpen(documentId))
            {
                throw new ArgumentException(string.Format(
                    WorkspacesResources._0_is_not_open,
                    this.GetDocumentName(documentId)));
            }
        }

        /// <summary>
        /// True if this workspace supports manually opening and closing documents.
        /// </summary>
        public virtual bool CanOpenDocuments => false;

        /// <summary>
        /// Open the specified document in the host environment.
        /// </summary>
        public virtual void OpenDocument(DocumentId documentId, bool activate = true)
        {
            this.CheckCanOpenDocuments();
        }

        protected void CheckCanOpenDocuments()
        {
            if (!this.CanOpenDocuments)
            {
                throw new NotSupportedException("This workspace does not support opening and closing documents");
            }
        }
    }
}
