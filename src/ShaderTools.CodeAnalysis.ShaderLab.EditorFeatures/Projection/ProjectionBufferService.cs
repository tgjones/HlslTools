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

            var cgBlockVisitor = new CgBlockVisitor(shaderLabSyntaxTree);
            cgBlockVisitor.Visit((SyntaxNode) shaderLabSyntaxTree.Root);
            var cgBlockSpans = cgBlockVisitor.CgBlockSpans;

            var snapshot = document.SourceText.FindCorrespondingEditorTextSnapshot();

            var dataBufferSpans = new List<object>();

            var primaryIndex = 0;
            foreach (var cgBlockSpan in cgBlockSpans)
            {
                var primarySpan = Span.FromBounds(primaryIndex, cgBlockSpan.Start);
                if (!primarySpan.IsEmpty)
                    dataBufferSpans.Add(snapshot.CreateTrackingSpan(
                        primarySpan, 
                        SpanTrackingMode.EdgeExclusive));

                var elisionBuffer = _projectionBufferFactoryService.CreateElisionBuffer(
                    null,
                    new NormalizedSnapshotSpanCollection(new SnapshotSpan(snapshot, cgBlockSpan)),
                    ElisionBufferOptions.None,
                    _hlslContentType);

                dataBufferSpans.Add(elisionBuffer.CurrentSnapshot.CreateTrackingSpan(
                    0, 
                    elisionBuffer.CurrentSnapshot.Length, 
                    SpanTrackingMode.EdgeInclusive));

                primaryIndex = cgBlockSpan.End;
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

        private sealed class CgBlockVisitor : SyntaxWalker
        {
            private readonly SyntaxTree _syntaxTree;

            public List<Span> CgBlockSpans { get; } = new List<Span>();

            public CgBlockVisitor(SyntaxTree syntaxTree)
            {
                _syntaxTree = syntaxTree;
            }

            public override void VisitCgProgram(CgProgramSyntax node)
            {
                AddBlockSpan(node.CgProgramKeyword, node.EndCgKeyword);
            }

            public override void VisitCgInclude(CgIncludeSyntax node)
            {
                AddBlockSpan(node.CgIncludeKeyword, node.EndCgKeyword);
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