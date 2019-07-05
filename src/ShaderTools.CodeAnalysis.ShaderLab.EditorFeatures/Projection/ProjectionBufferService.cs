using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.CodeAnalysis.Editor.Shared.Threading;
using ShaderTools.CodeAnalysis.Editor.Shared.Utilities;
using ShaderTools.CodeAnalysis.ShaderLab.Syntax;
using ShaderTools.CodeAnalysis.Shared.TestHooks;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Editor.ShaderLab.Projection
{
    /// <summary>
    /// Manages the projection buffer for ShaderLab files.
    /// 
    /// Graph:
    //      View Buffer [ContentType = Projection]
    //        |      \
    //        |    Secondary [ContentType = HLSL]
    //        |      /
    //       Disk Buffer [ContentType = ShaderLab]
    /// </summary>
    internal sealed class ProjectionBufferService : ForegroundThreadAffinitizedObject, IProjectionBufferService, IDisposable
    {
        private readonly IContentType _hlslContentType;
        private readonly IProjectionBufferFactoryService _projectionBufferFactoryService;

        private readonly IAsynchronousOperationListener _listener;
        private readonly IForegroundNotificationService _foregroundNotificationService;

        private readonly Workspace _workspace;

        private readonly AsynchronousSerialWorkQueue _workQueue;

        public ProjectionBufferService(
            IProjectionBufferFactoryService projectionBufferFactoryService,
            IContentTypeRegistryService contentTypeRegistryService,
            IForegroundNotificationService foregroundNotificationService,
            IAsynchronousOperationListener listener,
            Workspace workspace)
        {
            _projectionBufferFactoryService = projectionBufferFactoryService;

            _listener = listener;
            _foregroundNotificationService = foregroundNotificationService;

            _workspace = workspace;
            _workspace.DocumentOpened += OnDocumentChanged;
            _workspace.DocumentChanged += OnDocumentChanged;

            _hlslContentType = contentTypeRegistryService.GetContentType(LanguageNames.Hlsl);

            _workQueue = new AsynchronousSerialWorkQueue(new AsynchronousOperationListener());
        }

        private void OnDocumentChanged(object sender, DocumentEventArgs e)
        {
            if (e.Document.Language != LanguageNames.ShaderLab)
                return;

            _workQueue.CancelCurrentWork();

            _workQueue.EnqueueBackgroundTask(
                async ct =>
                {
                    // TODO: Figure out whether this is a destructive change.
                    // TODO: Incremental updates.
                    await UpdateBuffersAsync(e.Document, ct);
                },
                "UpdateBuffers",
                _workQueue.CancellationToken);
        }

        private async Task UpdateBuffersAsync(Document document, CancellationToken cancellationToken)
        {
            var shaderLabSyntaxTree = (SyntaxTree) await document.GetSyntaxTreeAsync(cancellationToken);

            var shaderBlockVisitor = new ShaderBlockVisitor(shaderLabSyntaxTree);
            shaderBlockVisitor.Visit((SyntaxNode) shaderLabSyntaxTree.Root);
            var shaderBlockSpans = shaderBlockVisitor.CgBlockSpans;

            var snapshot = document.SourceText.FindCorrespondingEditorTextSnapshot();

            var dataBufferSpans = new List<object>();

            var primaryIndex = 0;
            foreach (var shaderBlockSpan in shaderBlockSpans)
            {
                var primarySpan = Span.FromBounds(primaryIndex, shaderBlockSpan.Start);
                if (!primarySpan.IsEmpty)
                    dataBufferSpans.Add(snapshot.CreateTrackingSpan(
                        primarySpan, 
                        SpanTrackingMode.EdgeExclusive));

                var elisionBuffer = _projectionBufferFactoryService.CreateElisionBuffer(
                    null,
                    new NormalizedSnapshotSpanCollection(new SnapshotSpan(snapshot, shaderBlockSpan)),
                    ElisionBufferOptions.None,
                    _hlslContentType);

                dataBufferSpans.Add(elisionBuffer.CurrentSnapshot.CreateTrackingSpan(
                    0, 
                    elisionBuffer.CurrentSnapshot.Length, 
                    SpanTrackingMode.EdgeInclusive));

                primaryIndex = shaderBlockSpan.End;
            }

            // Last span.
            {
                var primarySpan = Span.FromBounds(primaryIndex, snapshot.Length);
                if (!primarySpan.IsEmpty)
                    dataBufferSpans.Add(snapshot.CreateTrackingSpan(primarySpan, SpanTrackingMode.EdgeExclusive));
            }

            // TODO: Make this a bit more type-safe.
            var viewBuffer = snapshot.TextBuffer.Properties.GetProperty<IProjectionBuffer>(typeof(IProjectionBuffer));

            _foregroundNotificationService.RegisterNotification(
                () =>
                {
                    viewBuffer.ReplaceSpans(
                        0,
                        viewBuffer.CurrentSnapshot.SpanCount,
                        dataBufferSpans,
                        EditOptions.None,
                        null);
                }, 
                _listener.BeginAsyncOperation("ReplaceProjectionBufferSpans"), 
                cancellationToken);
            
        }

        private sealed class ShaderBlockVisitor : SyntaxWalker
        {
            private readonly SyntaxTree _syntaxTree;

            public List<Span> CgBlockSpans { get; } = new List<Span>();

            public ShaderBlockVisitor(SyntaxTree syntaxTree)
            {
                _syntaxTree = syntaxTree;
            }

            public override void VisitShaderProgram(ShaderProgramSyntax node)
            {
                AddBlockSpan(node.BeginProgramKeyword, node.EndProgramKeyword);
            }

            public override void VisitShaderInclude(ShaderIncludeSyntax node)
            {
                AddBlockSpan(node.BeginIncludeKeyword, node.EndIncludeKeyword);
            }

            private void AddBlockSpan(SyntaxToken startToken, SyntaxToken endToken)
            {
                var start = _syntaxTree.GetSourceFilePoint(startToken.SourceRange.End);
                var end = _syntaxTree.GetSourceFilePoint(endToken.SourceRange.Start);

                var span = Span.FromBounds(start, end);

                CgBlockSpans.Add(span);
            }
        }

        public void Dispose()
        {
            _workspace.DocumentOpened -= OnDocumentChanged;
            _workspace.DocumentChanged -= OnDocumentChanged;
        }
    }
}