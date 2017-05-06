//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
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

        public event EventHandler<DocumentEventArgs> DocumentOpened;
        public event EventHandler<DocumentEventArgs> DocumentClosed;

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

        protected Document CreateDocument(DocumentId documentId, string languageName, SourceText sourceText)
        {
            var languageServices = _services.GetLanguageServices(languageName);
            return new Document(languageServices, documentId, sourceText);
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
        public ConfigFile LoadConfigFile(string directory)
        {
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
    }
}
