using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Properties;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.Utilities.Diagnostics;

namespace ShaderTools.CodeAnalysis
{
    /// <summary>
    /// Immutable collection of immutable documents. Equivalent to Roslyn's Solution or Project.
    /// </summary>
    public sealed class WorkspaceDocuments
    {
        private readonly ImmutableDictionary<DocumentId, Document> _idToDocumentMap;

        public IEnumerable<DocumentId> DocumentIds => _idToDocumentMap.Keys;

        internal IEnumerable<Document> Documents => _idToDocumentMap.Values;

        internal WorkspaceDocuments(ImmutableDictionary<DocumentId, Document> idToDocumentMap)
        {
            _idToDocumentMap = idToDocumentMap;
        }

        /// <summary>
        /// True if the solution contains the document in one of its projects
        /// </summary>
        public bool ContainsDocument(DocumentId documentId)
        {
            return
                documentId != null &&
                _idToDocumentMap.ContainsKey(documentId);
        }

        public Document GetDocument(DocumentId documentId)
        {
            return _idToDocumentMap.GetValueOrDefault(documentId);
        }

        public ImmutableArray<Document> GetDocumentsWithFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return ImmutableArray.Create<Document>();
            }

            return _idToDocumentMap.Values
                .Where(x => string.Equals(x.FilePath, filePath, StringComparison.OrdinalIgnoreCase))
                .ToImmutableArray();
        }

        public Document GetDocumentWithFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            var documentId = _idToDocumentMap.Values
                .FirstOrDefault(x => string.Equals(x.FilePath, filePath, StringComparison.OrdinalIgnoreCase))
                ?.Id;

            return (documentId != null)
                ? GetDocument(documentId)
                : null;
        }

        /// <summary>
        /// Gets the document in this solution with the specified syntax tree.
        /// </summary>
        public Document GetDocument(SyntaxTreeBase syntaxTree)
        {
            if (syntaxTree != null)
            {
                // is this tree known to be associated with a document?
                var docId = Document.GetDocumentIdForTree(syntaxTree);
                if (docId != null)
                {
                    // does this solution even have the document?
                    var document = this.GetDocument(docId);
                    if (document != null)
                    {
                        // does this document really have the syntax tree?
                        if (document.TryGetSyntaxTree(out var documentTree) && documentTree == syntaxTree)
                        {
                            return document;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a new document with the document specified updated to have the text
        /// specified.
        /// </summary>
        public WorkspaceDocuments WithDocumentText(DocumentId documentId, SourceText text)
        {
            if (documentId == null)
            {
                throw new ArgumentNullException(nameof(documentId));
            }

            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            CheckContainsDocument(documentId);

            var oldDocument = this.GetDocument(documentId);
            if (oldDocument.SourceText == text)
            {
                return this;
            }

            var newDocument = oldDocument.WithText(text);

            var newState = _idToDocumentMap.SetItem(documentId, newDocument);

            return new WorkspaceDocuments(newState);
        }

        private void CheckContainsDocument(DocumentId documentId)
        {
            Contract.Requires(this.ContainsDocument(documentId));

            if (!this.ContainsDocument(documentId))
            {
                throw new InvalidOperationException(WorkspacesResources.The_workspace_does_not_contain_the_specified_document);
            }
        }

        /// <summary>
        /// Creates a new solution instance that no longer includes the specified document.
        /// </summary>
        public WorkspaceDocuments RemoveDocument(DocumentId documentId)
        {
            var newState = _idToDocumentMap.Remove(documentId);
            if (newState == _idToDocumentMap)
            {
                return this;
            }

            return new WorkspaceDocuments(newState);
        }

        /// <summary>
        /// Create a new solution instance with the corresponding project updated to include a new 
        /// document instanced defined by the document info.
        /// </summary>
        public WorkspaceDocuments AddDocument(Document document)
        {
            var newState = _idToDocumentMap.Add(document.Id, document);
            if (newState == _idToDocumentMap)
            {
                return this;
            }

            return new WorkspaceDocuments(newState);
        }
    }
}
