//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShaderTools.LanguageServer.Protocol.LanguageServer;
using ShaderTools.LanguageServer.Protocol.MessageProtocol;
using ShaderTools.LanguageServer.Protocol.MessageProtocol.Channel;
using ShaderTools.EditorServices.Utility;
using System.IO;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.LanguageServer.Protocol.Server
{
    public abstract class LanguageServerBase : ProtocolEndpoint
    {
        private readonly static string DiagnosticSourceName = "ShaderToolsEditorServices";

        private readonly ChannelBase serverChannel;
        private readonly LanguageServerWorkspace _workspace;

        private string _workspacePath;

        private static CancellationTokenSource existingRequestCancellation;

        public LanguageServerBase(ChannelBase serverChannel, LanguageServerWorkspace workspace)
            :  base(serverChannel, MessageProtocolType.LanguageServer)
        {
            this.serverChannel = serverChannel;
            _workspace = workspace;
        }

        protected override Task OnStart()
        {
            // Register handlers for server lifetime messages
            this.SetRequestHandler(ShutdownRequest.Type, this.HandleShutdownRequest);
            this.SetEventHandler(ExitNotification.Type, this.HandleExitNotification);

            // Initialize the implementation class
            this.Initialize();

            return Task.FromResult(true);
        }

        protected override async Task OnStop()
        {
            await this.Shutdown();
        }

        /// <summary>
        /// Overridden by the subclass to provide initialization
        /// logic after the server channel is started.
        /// </summary>
        protected void Initialize()
        {
            this.SetRequestHandler(InitializeRequest.Type, this.HandleInitializeRequest);

            this.SetEventHandler(DidOpenTextDocumentNotification.Type, this.HandleDidOpenTextDocumentNotification);
            this.SetEventHandler(DidCloseTextDocumentNotification.Type, this.HandleDidCloseTextDocumentNotification);
            this.SetEventHandler(DidChangeTextDocumentNotification.Type, this.HandleDidChangeTextDocumentNotification);
        }

        /// <summary>
        /// Can be overridden by the subclass to provide shutdown
        /// logic before the server exits.  Subclasses do not need
        /// to invoke or return the value of the base implementation.
        /// </summary>
        protected Task Shutdown()
        {
            Logger.Write(LogLevel.Normal, "Language service is shutting down...");

            return Task.FromResult(true);
        }

        private async Task HandleInitializeRequest(
            InitializeRequest initializeParams,
            RequestContext<InitializeResult> requestContext)
        {
            // Grab the workspace path from the parameters
            _workspacePath = initializeParams.RootPath;

            await requestContext.SendResult(
                new InitializeResult
                {
                    Capabilities = new ServerCapabilities
                    {
                        TextDocumentSync = TextDocumentSyncKind.Incremental,
                        //DefinitionProvider = true,
                        //ReferencesProvider = true,
                        //DocumentHighlightProvider = true,
                        //DocumentSymbolProvider = true,
                        //WorkspaceSymbolProvider = true,
                        //HoverProvider = true,
                        //CodeActionProvider = true,
                        //CompletionProvider = new CompletionOptions
                        //{
                        //    ResolveProvider = true,
                        //    TriggerCharacters = new string[] { ".", "-", ":", "\\" }
                        //},
                        //SignatureHelpProvider = new SignatureHelpOptions
                        //{
                        //    TriggerCharacters = new string[] { " " } // TODO: Other characters here?
                        //}
                    }
                });
        }

        private Task HandleDidOpenTextDocumentNotification(
            DidOpenTextDocumentNotification openParams,
            EventContext eventContext)
        {
            var openedDocument = _workspace.OpenDocument(
                CreateDocumentId(openParams.Uri),
                SourceText.From(openParams.Text));

            // TODO: Get all recently edited files in the workspace
            this.RunScriptDiagnostics(new Document[] { openedDocument });

            Logger.Write(LogLevel.Verbose, "Finished opening document.");

            return Task.FromResult(true);
        }

        private async Task HandleDidCloseTextDocumentNotification(
            TextDocumentIdentifier closeParams,
            EventContext eventContext)
        {
            // Find and close the file in the current session
            var fileToClose = _workspace.GetDocument(CreateDocumentId(closeParams.Uri));

            if (fileToClose != null)
            {
                _workspace.CloseDocument(fileToClose.Id);
                await ClearMarkers(fileToClose, eventContext);
            }

            Logger.Write(LogLevel.Verbose, "Finished closing document.");
        }

        private Task HandleDidChangeTextDocumentNotification(
            DidChangeTextDocumentParams textChangeParams,
            EventContext eventContext)
        {
            var changedFiles = new List<Document>();

            // A text change notification can batch multiple change requests
            foreach (var textChange in textChangeParams.ContentChanges)
            {
                var fileToChange = _workspace.GetDocument(CreateDocumentId(textChangeParams.Uri));
                if (fileToChange == null)
                    continue;

                var changedFile = _workspace.UpdateDocument(fileToChange,
                    GetFileChangeDetails(
                        fileToChange,
                        textChange.Range.Value,
                        textChange.Text));

                changedFiles.Add(changedFile);
            }

            // TODO: Get all recently edited files in the workspace
            this.RunScriptDiagnostics(changedFiles.ToArray());

            return Task.FromResult(true);
        }

        private static TextChange GetFileChangeDetails(Document document, Range changeRange, string insertString)
        {
            var startPosition = document.SourceText.Lines.GetPosition(new LinePosition(changeRange.Start.Line, changeRange.Start.Character));
            var endPosition = document.SourceText.Lines.GetPosition(new LinePosition(changeRange.End.Line, changeRange.End.Character));

            return new TextChange(TextSpan.FromBounds(startPosition, endPosition), insertString);
        }

        private async Task ClearMarkers(Document scriptFile, EventContext eventContext)
        {
            // send empty diagnostic markers to clear any markers associated with the given file
            await PublishScriptDiagnostics(
                    scriptFile,
                    new CodeAnalysis.Diagnostics.Diagnostic[0],
                    eventContext);
        }

        private static async Task PublishScriptDiagnostics(
            Document scriptFile,
            CodeAnalysis.Diagnostics.Diagnostic[] markers,
            EventContext eventContext)
        {
            await PublishScriptDiagnostics(
                scriptFile, () => throw new NotSupportedException(),
                markers,
                eventContext.SendEvent);
        }

        private Task RunScriptDiagnostics(
            Document[] filesToAnalyze)
        {
            return RunScriptDiagnostics(filesToAnalyze, this.SendEvent);
        }

        private Task RunScriptDiagnostics(
            Document[] filesToAnalyze,
            Func<EventType<PublishDiagnosticsNotification>, PublishDiagnosticsNotification, Task> eventSender)
        {
            // If there's an existing task, attempt to cancel it
            try
            {
                if (existingRequestCancellation != null)
                {
                    // Try to cancel the request
                    existingRequestCancellation.Cancel();

                    // If cancellation didn't throw an exception,
                    // clean up the existing token
                    existingRequestCancellation.Dispose();
                    existingRequestCancellation = null;
                }
            }
            catch (Exception e)
            {
                // TODO: Catch a more specific exception!
                Logger.Write(
                    LogLevel.Error,
                    string.Format(
                        "Exception while canceling analysis task:\n\n{0}",
                        e.ToString()));

                TaskCompletionSource<bool> cancelTask = new TaskCompletionSource<bool>();
                cancelTask.SetCanceled();
                return cancelTask.Task;
            }

            // Create a fresh cancellation token and then start the task.
            // We create this on a different TaskScheduler so that we
            // don't block the main message loop thread.
            // TODO: Is there a better way to do this?
            existingRequestCancellation = new CancellationTokenSource();
            Task.Factory.StartNew(
                () =>
                    DelayThenInvokeDiagnostics(
                        750,
                        filesToAnalyze,
                        eventSender,
                        existingRequestCancellation.Token),
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default);

            return Task.FromResult(true);
        }

        private static async Task DelayThenInvokeDiagnostics(
            int delayMilliseconds,
            Document[] filesToAnalyze,
            Func<EventType<PublishDiagnosticsNotification>, PublishDiagnosticsNotification, Task> eventSender,
            CancellationToken cancellationToken)
        {
            // First of all, wait for the desired delay period before
            // analyzing the provided list of files
            try
            {
                await Task.Delay(delayMilliseconds, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // If the task is cancelled, exit directly
                return;
            }

            // If we've made it past the delay period then we don't care
            // about the cancellation token anymore.  This could happen
            // when the user stops typing for long enough that the delay
            // period ends but then starts typing while analysis is going
            // on.  It makes sense to send back the results from the first
            // delay period while the second one is ticking away.

            // Get the requested files
            foreach (var scriptFile in filesToAnalyze)
            {
                Logger.Write(LogLevel.Verbose, "Analyzing script file: " + scriptFile.FilePath);

                var syntaxTree = await scriptFile.GetSyntaxTreeAsync(CancellationToken.None);

                var syntaxDiagnostics = syntaxTree.GetDiagnostics();
                var semanticDiagnostics = (await scriptFile.GetSemanticModelAsync(CancellationToken.None)).GetDiagnostics();

                Logger.Write(LogLevel.Verbose, "Analysis complete.");

                await PublishScriptDiagnostics(
                    scriptFile, () => syntaxTree,
                    syntaxDiagnostics.Concat(semanticDiagnostics).ToArray(),
                    eventSender);
            }
        }

        private static async Task PublishScriptDiagnostics(
            Document scriptFile, Func<SyntaxTreeBase> syntaxTree,
            CodeAnalysis.Diagnostics.Diagnostic[] markers,
            Func<EventType<PublishDiagnosticsNotification>, PublishDiagnosticsNotification, Task> eventSender)
        {
            List<Diagnostic> diagnostics = new List<Diagnostic>();

            foreach (var marker in markers)
            {
                // Does the marker contain a correction?
                Diagnostic markerDiagnostic = GetDiagnosticFromMarker(syntaxTree(), marker);

                diagnostics.Add(markerDiagnostic);
            }

            // Always send syntax and semantic errors.  We want to
            // make sure no out-of-date markers are being displayed.
            await eventSender(
                PublishDiagnosticsNotification.Type,
                new PublishDiagnosticsNotification
                {
                    Uri = scriptFile.FilePath,
                    Diagnostics = diagnostics.ToArray()
                });
        }

        private static Diagnostic GetDiagnosticFromMarker(SyntaxTreeBase syntaxTree, CodeAnalysis.Diagnostics.Diagnostic diagnostic)
        {
            var sourceFileSpan = syntaxTree.GetSourceFileSpan(diagnostic.SourceRange);
            var linePositionSpan = syntaxTree.Text.Lines.GetLinePositionSpan(sourceFileSpan.Span);

            return new Diagnostic
            {
                Severity = MapDiagnosticSeverity(diagnostic.Severity),
                Message = diagnostic.Message,
                Code = diagnostic.Descriptor.Id,
                Source = DiagnosticSourceName,
                Range = new Range
                {
                    Start = new Position
                    {
                        Line = linePositionSpan.Start.Line,
                        Character = linePositionSpan.Start.Character
                    },
                    End = new Position
                    {
                        Line = linePositionSpan.End.Line,
                        Character = linePositionSpan.End.Character
                    }
                }
            };
        }

        private static DiagnosticSeverity MapDiagnosticSeverity(CodeAnalysis.Diagnostics.DiagnosticSeverity severity)
        {
            switch (severity)
            {
                case CodeAnalysis.Diagnostics.DiagnosticSeverity.Error:
                    return DiagnosticSeverity.Error;

                case CodeAnalysis.Diagnostics.DiagnosticSeverity.Warning:
                    return DiagnosticSeverity.Warning;

                default:
                    return DiagnosticSeverity.Error;
            }
        }

        private async Task HandleShutdownRequest(
            object shutdownParams,
            RequestContext<object> requestContext)
        {
            // Allow the implementor to shut down gracefully
            await this.Shutdown();

            await requestContext.SendResult(new object());
        }

        private async Task HandleExitNotification(
            object exitParams,
            EventContext eventContext)
        {
            // Stop the server channel
            await this.Stop();
        }

        private static bool IsPathInMemory(string filePath)
        {
            // When viewing PowerShell files in the Git diff viewer, VS Code
            // sends the contents of the file at HEAD with a URI that starts
            // with 'inmemory'.  Untitled files which have been marked of
            // type PowerShell have a path starting with 'untitled'.
            return
                filePath.StartsWith("inmemory") ||
                filePath.StartsWith("untitled") ||
                filePath.StartsWith("private") ||
                filePath.StartsWith("git");

            // TODO #342: Remove 'private' and 'git' and then add logic to
            // throw when any unsupported file URI scheme is encountered.
        }

        private string ResolveFilePath(string filePath)
        {
            if (!IsPathInMemory(filePath))
            {
                if (filePath.StartsWith(@"file://"))
                {
                    // Client sent the path in URI format, extract the local path
                    Uri fileUri = new Uri(Uri.UnescapeDataString(filePath));
                    filePath = fileUri.LocalPath;
                }

                if (!Path.IsPathRooted(filePath))
                {
                    filePath = Path.Combine(_workspacePath, filePath);
                }

                // Get the absolute file path
                filePath = Path.GetFullPath(filePath);
            }

            Logger.Write(LogLevel.Verbose, "Resolved path: " + filePath);

            return filePath;
        }

        private DocumentId CreateDocumentId(string filePath)
        {
            return new DocumentId(ResolveFilePath(filePath));
        }
    }
}

