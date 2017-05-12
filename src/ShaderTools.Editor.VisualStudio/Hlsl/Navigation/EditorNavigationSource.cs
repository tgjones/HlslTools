using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using ShaderTools.CodeAnalysis.Editor.Shared.Threading;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Shared.TestHooks;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Editor.VisualStudio.Core.Glyphs;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Navigation
{
    internal sealed class EditorNavigationSource
    {
        private readonly ITextBuffer _textBuffer;
        private readonly DispatcherGlyphService _glyphService;
        private List<EditorTypeNavigationTarget> _navigationTargets;

        private readonly AsynchronousSerialWorkQueue _workQueue;

        public EditorNavigationSource(ITextBuffer textBuffer, DispatcherGlyphService glyphService)
        {
            _textBuffer = textBuffer;
            _glyphService = glyphService;

            _navigationTargets = new List<EditorTypeNavigationTarget>();

            _workQueue = new AsynchronousSerialWorkQueue(new AsynchronousOperationListener());

            // TODO: We don't unsubscribe from this.
            _textBuffer.Changed += OnTextBufferChanged;

            OnTextBufferChanged();
        }

        private void OnTextBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            OnTextBufferChanged();
        }

        private void OnTextBufferChanged()
        {
            _workQueue.CancelCurrentWork();

            var cancellationToken = _workQueue.CancellationToken;

            _workQueue.EnqueueBackgroundTask(
                ct => InvalidateTargetsAsync(_textBuffer.CurrentSnapshot, ct),
                "InvalidateTargets",
                cancellationToken);
        }

        private async Task InvalidateTargetsAsync(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            var navigationTargets = new List<EditorTypeNavigationTarget>();

            var document = snapshot.AsText().GetOpenDocumentInCurrentContextWithChanges();
            if (document == null)
                return;

            var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);

            var navigationTargetsVisitor = new NavigationTargetsVisitor(snapshot, (SyntaxTree) syntaxTree, _glyphService, cancellationToken);
            navigationTargets.AddRange(navigationTargetsVisitor.GetTargets((CompilationUnitSyntax) syntaxTree.Root));

            _navigationTargets = navigationTargets;

            OnNavigationTargetsChanged(EventArgs.Empty);
        }

        public event EventHandler NavigationTargetsChanged;

        public IEnumerable<EditorTypeNavigationTarget> GetNavigationTargets()
        {
            return _navigationTargets;
        }

        private void OnNavigationTargetsChanged(EventArgs e)
        {
            NavigationTargetsChanged?.Invoke(this, e);
        }
    }
}