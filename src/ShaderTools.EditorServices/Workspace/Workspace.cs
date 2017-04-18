//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ShaderTools.Core.Options;
using ShaderTools.Core.Text;
using ShaderTools.EditorServices.Utility;
using ShaderTools.EditorServices.Workspace.Host;

namespace ShaderTools.EditorServices.Workspace
{
    public abstract class Workspace
    {
        private readonly HostWorkspaceServices _services;
        //private readonly HostLanguageServices _languageServices;

        private readonly SemaphoreSlim _serializationLock = new SemaphoreSlim(initialCount: 1);

        private ImmutableDictionary<DocumentId, Document> _openDocuments = ImmutableDictionary<DocumentId, Document>.Empty;
        private ImmutableDictionary<string, ConfigFile> _configFiles = ImmutableDictionary<string, ConfigFile>.Empty;

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

        //protected ImmutableDictionary<DocumentId, Document> SetCurrentDocuments(ImmutableDictionary<DocumentId, Document> documents)
        //{
        //    var currentDocuments = Volatile.Read(ref _latestDocuments);
        //    if (documents == currentDocuments)
        //    {
        //        // No change
        //        return documents;
        //    }

        //    while (true)
        //    {
        //        var newDocuments = documents.WithNewWorkspace(this, currentDocuments.WorkspaceVersion + 1);
        //        var replacedDocuments = ImmutableInterlocked.InterlockedExchange(ref _latestDocuments, newDocuments, currentDocuments);
        //        if (replacedDocuments == currentDocuments)
        //        {
        //            return newDocuments;
        //        }

        //        currentDocuments = replacedDocuments;
        //    }
        //}

        ///// <summary>
        ///// Gets an open file in the workspace.  If the file isn't open but
        ///// exists on the filesystem, load and return it.
        ///// </summary>
        ///// <param name="filePath">The file path at which the script resides.</param>
        ///// <exception cref="FileNotFoundException">
        ///// <paramref name="filePath"/> is not found.
        ///// </exception>
        ///// <exception cref="ArgumentException">
        ///// <paramref name="filePath"/> contains a null or empty string.
        ///// </exception>
        //public Document GetFile(string filePath)
        //{
        //    Validate.IsNotNullOrEmptyString(nameof(filePath), filePath);

        //    // Resolve the full file path 
        //    string resolvedFilePath = this.ResolveFilePath(filePath);
        //    string keyName = resolvedFilePath.ToLower();

        //    // Make sure the file isn't already loaded into the workspace
        //    Document scriptFile = null;
        //    if (!_workspaceFiles.TryGetValue(keyName, out scriptFile))
        //    {
        //        // This method allows FileNotFoundException to bubble up 
        //        // if the file isn't found.
        //        using (FileStream fileStream = new FileStream(resolvedFilePath, FileMode.Open, FileAccess.Read))
        //        using (StreamReader streamReader = new StreamReader(fileStream, Encoding.UTF8))
        //        {
        //            var text = SourceText.From(streamReader.ReadToEnd(), resolvedFilePath);
        //            scriptFile = CreateDocument(text, filePath);

        //            _workspaceFiles = _workspaceFiles.Add(keyName, scriptFile);
        //        }

        //        Logger.Write(LogLevel.Verbose, "Opened file on disk: " + resolvedFilePath);
        //    }

        //    return scriptFile;
        //}

        //public bool TryGetDocument(string filePath, out Document document)
        //{
        //    var keyName = ResolveFilePath(filePath).ToLower();
        //    return _workspaceFiles.TryGetValue(keyName, out document);
        //}

        ///// <summary>
        ///// Gets a new <see cref="Document"/> instance which is identified by the given file
        ///// path and initially contains the given buffer contents.
        ///// </summary>
        ///// <param name="filePath">The file path for which a buffer will be retrieved.</param>
        ///// <param name="initialBuffer">The initial buffer contents if there is not an existing ScriptFile for this path.</param>
        ///// <returns>A ScriptFile instance for the specified path.</returns>
        //public Document GetFileBuffer(string filePath, string initialBuffer)
        //{
        //    Validate.IsNotNullOrEmptyString(nameof(filePath), filePath);

        //    // Resolve the full file path 
        //    string resolvedFilePath = this.ResolveFilePath(filePath);
        //    string keyName = resolvedFilePath.ToLower();

        //    // Make sure the file isn't already loaded into the workspace
        //    Document scriptFile = null;
        //    if (!_workspaceFiles.TryGetValue(keyName, out scriptFile) && initialBuffer != null)
        //    {
        //        scriptFile = CreateDocument(SourceText.From(initialBuffer, resolvedFilePath), filePath);

        //        _workspaceFiles = _workspaceFiles.Add(keyName, scriptFile);

        //        Logger.Write(LogLevel.Verbose, "Opened file as in-memory buffer: " + resolvedFilePath);
        //    }

        //    return scriptFile;
        //}

        protected void OnDocumentOpened(Document document)
        {
            ImmutableInterlocked.AddOrUpdate(ref _openDocuments, document.Id, document, (k, v) => document);
        }

        protected void OnDocumentClosed(DocumentId documentId)
        {
            ImmutableInterlocked.TryRemove(ref _openDocuments, documentId, out var _);
        }

        protected void OnDocumentRenamed(DocumentId oldDocumentId, DocumentId newDocumentId)
        {
            if (!ImmutableInterlocked.TryRemove(ref _openDocuments, oldDocumentId, out var oldDocument))
                return;

            ImmutableInterlocked.TryAdd(ref _openDocuments, newDocumentId, oldDocument.WithId(newDocumentId));
        }

        protected Document OnDocumentTextChanged(Document document, SourceText newText)
        {
            return ImmutableInterlocked.AddOrUpdate(
                ref _openDocuments,
                document.Id,
                k => document.WithText(newText),
                (k, v) => v.WithText(newText));
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
