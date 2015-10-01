using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HlslTools.Syntax;
using HlslTools.VisualStudio.Glyphs;
using HlslTools.VisualStudio.Parsing;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Text;

namespace HlslTools.VisualStudio.Navigation
{
    internal sealed class EditorNavigationSource : IBackgroundParserSyntaxTreeHandler
    {
        private readonly ITextBuffer _textBuffer;
        private readonly DispatcherGlyphService _glyphService;
        private List<EditorTypeNavigationTarget> _navigationTargets;

        public EditorNavigationSource(ITextBuffer textBuffer, BackgroundParser backgroundParser, DispatcherGlyphService glyphService)
        {
            _textBuffer = textBuffer;
            _glyphService = glyphService;

            _navigationTargets = new List<EditorTypeNavigationTarget>();

            backgroundParser.RegisterSyntaxTreeHandler(BackgroundParserHandlerPriority.Medium, this);
        }

        public async Task Initialize()
        {
            await Task.Run(async () =>
            {
                var snapshot = _textBuffer.CurrentSnapshot;
                var snapshotSyntaxTree = new SnapshotSyntaxTree(snapshot, snapshot.GetSyntaxTree(CancellationToken.None));
                await InvalidateTargets(snapshotSyntaxTree, CancellationToken.None);
            });
        }

        private async Task InvalidateTargets(SnapshotSyntaxTree snapshotSyntaxTree, CancellationToken cancellationToken)
        {
            var navigationTargets = new List<EditorTypeNavigationTarget>();

            var navigationTargetsVisitor = new NavigationTargetsVisitor(snapshotSyntaxTree.Snapshot, _glyphService, cancellationToken);

            await Task.Run(() => navigationTargets.AddRange(navigationTargetsVisitor.GetTargets((CompilationUnitSyntax) snapshotSyntaxTree.SyntaxTree.Root)), cancellationToken);

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

        async Task IBackgroundParserSyntaxTreeHandler.OnSyntaxTreeAvailable(SnapshotSyntaxTree snapshotSyntaxTree, CancellationToken cancellationToken)
        {
            await InvalidateTargets(snapshotSyntaxTree, cancellationToken);
        }
    }
}