//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Properties;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Utilities.Threading;

namespace ShaderTools.CodeAnalysis
{
    public abstract partial class Workspace
    {
        private readonly HostWorkspaceServices _services;

        // forces serialization of mutation calls from host (OnXXX methods). Must take this lock before taking stateLock.
        private readonly SemaphoreSlim _serializationLock = new SemaphoreSlim(initialCount: 1);

        // this lock guards all the mutable fields (do not share lock with derived classes)
        private readonly NonReentrantLock _stateLock = new NonReentrantLock(useThisInstanceForSynchronization: true);

        // Current documents.
        private WorkspaceDocuments _latestDocuments;

        private ImmutableDictionary<string, ConfigFile> _configFiles = ImmutableDictionary<string, ConfigFile>.Empty;

        public HostWorkspaceServices Services => _services;

        private readonly IWorkspaceTaskScheduler _taskQueue;

        protected Workspace(HostServices host)
        {
            _services = host.CreateWorkspaceServices(this);

            // queue used for sending events
            var workspaceTaskSchedulerFactory = _services.GetRequiredService<IWorkspaceTaskSchedulerFactory>();
            _taskQueue = workspaceTaskSchedulerFactory.CreateEventingTaskQueue();

            // initialize with empty document set
            _latestDocuments = new WorkspaceDocuments(ImmutableDictionary<DocumentId, Document>.Empty);
        }

        /// <summary>
        /// The current documents. 
        /// 
        /// <see cref="WorkspaceDocuments"/> is an immutable model of the current set of open documents,
        /// and referenced (i.e. #include'd) documents.
        /// It provides access to source text, syntax trees and semantics.
        /// 
        /// This property may change as the workspace reacts to changes in the environment.
        /// </summary>
        public WorkspaceDocuments CurrentDocuments
        {
            get
            {
                return Volatile.Read(ref _latestDocuments);
            }
        }

        protected Document CreateDocument(DocumentId documentId, string languageName, SourceFile file)
        {
            var languageServices = _services.GetLanguageServices(languageName);
            return new Document(languageServices, documentId, file);
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

        protected void OnDocumentTextChanged(DocumentId documentId, SourceText newText)
        {
            using (_serializationLock.DisposableWait())
            {
                CheckDocumentIsInCurrentSolution(documentId);

                var oldSolution = this.CurrentDocuments;
                var newSolution = this.SetCurrentDocuments(oldSolution.WithDocumentText(documentId, newText));

                var newDocument = newSolution.GetDocument(documentId);
                this.OnDocumentTextChanged(newDocument);

                DocumentChanged?.Invoke(this, new DocumentEventArgs(newDocument));
            }
        }

        /// <summary>
        /// Throws an exception if a document is not part of the current solution.
        /// </summary>
        protected void CheckDocumentIsInCurrentSolution(DocumentId documentId)
        {
            if (this.CurrentDocuments.GetDocument(documentId) == null)
            {
                throw new ArgumentException(string.Format(
                    WorkspacesResources._0_is_not_part_of_the_workspace,
                    this.GetDocumentName(documentId)));
            }
        }

        // TODO: Refactor this.
        public ConfigFile LoadConfigFile(SourceFile file)
        {
            var directory = Path.GetDirectoryName(file.FilePath);

            if (directory == null)
                return new ConfigFile();

            return ImmutableInterlocked.GetOrAdd(
                ref _configFiles, 
                directory.ToLower(), 
                x => ConfigFileLoader.LoadAndMergeConfigFile(x));
        }

        /// <summary>
        /// Executes an action as a background task, as part of a sequential queue of tasks.
        /// </summary>
        protected internal Task ScheduleTask(Action action, string taskName = "Workspace.Task")
        {
            return _taskQueue.ScheduleTask(action, taskName);
        }

        /// <summary>
        /// Gets the name to use for a document in an error message.
        /// </summary>
        protected virtual string GetDocumentName(DocumentId documentId)
        {
            var document = this.CurrentDocuments.GetDocument(documentId);
            var name = document != null ? document.Name : "<Document" + documentId.Id + ">";
            return name;
        }

        /// <summary>
        /// Sets the <see cref="CurrentDocuments"/> of this workspace. This method does not raise a <see cref="WorkspaceChanged"/> event.
        /// </summary>
        protected WorkspaceDocuments SetCurrentDocuments(WorkspaceDocuments documents)
        {
            var currentDocuments = Volatile.Read(ref _latestDocuments);
            if (documents == currentDocuments)
            {
                // No change
                return documents;
            }

            while (true)
            {
                var replacedSolution = Interlocked.CompareExchange(ref _latestDocuments, documents, currentDocuments);
                if (replacedSolution == currentDocuments)
                {
                    return documents;
                }

                currentDocuments = replacedSolution;
            }
        }

        /// <summary>
        /// Apply changes made to a solution back to the workspace.
        /// 
        /// The specified solution must be one that originated from this workspace. If it is not, or the workspace
        /// has been updated since the solution was obtained from the workspace, then this method returns false. This method
        /// will still throw if the solution contains changes that are not supported according to the <see cref="CanApplyChange(ApplyChangesKind)"/>
        /// method.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown if the solution contains changes not supported according to the
        /// <see cref="CanApplyChange(ApplyChangesKind)"/> method.</exception>
        public virtual bool TryApplyChanges(WorkspaceDocuments newDocuments)
        {
            var currentDocuments = CurrentDocuments;

            foreach (var newDocument in newDocuments.Documents)
            {
                var currentDocument = currentDocuments.GetDocument(newDocument.Id);
                if (currentDocument != null && currentDocument.SourceText != newDocument.SourceText)
                    ApplyDocumentTextChanged(currentDocument.Id, newDocument.SourceText);
            }

            return true;
        }

        protected virtual void ApplyDocumentTextChanged(DocumentId id, SourceText text)
        {

        }

        /// <summary>
        /// Gets or sets the set of all global options.
        /// </summary>
        public OptionSet Options
        {
            get
            {
                return _services.GetService<IOptionService>().GetOptions();
            }

            set
            {
                _services.GetService<IOptionService>().SetOptions(value);
            }
        }
    }
}
