using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.VisualStudio.Core.Parsing;
using ShaderTools.VisualStudio.Core.Util;
using ShaderTools.VisualStudio.Hlsl.Glyphs;
using ShaderTools.VisualStudio.Hlsl.Parsing;
using ShaderTools.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.VisualStudio.Hlsl.Navigation
{
    internal sealed class EditorNavigationSource
    {
        private readonly ITextBuffer _textBuffer;
        private readonly DispatcherGlyphService _glyphService;
        private List<EditorTypeNavigationTarget> _navigationTargets;

        public EditorNavigationSource(ITextBuffer textBuffer, BackgroundParser backgroundParser, DispatcherGlyphService glyphService)
        {
            _textBuffer = textBuffer;
            _glyphService = glyphService;

            _navigationTargets = new List<EditorTypeNavigationTarget>();

            backgroundParser.SubscribeToThrottledSyntaxTreeAvailable(BackgroundParserSubscriptionDelay.Medium,
                async x => await ExceptionHelper.TryCatchCancellation(async () => await InvalidateTargets(x.Snapshot, x.CancellationToken)));
        }

        public async void Initialize()
        {
            await Task.Run(async () => await InvalidateTargets(_textBuffer.CurrentSnapshot, CancellationToken.None));
        }

        private async Task InvalidateTargets(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            var navigationTargets = new List<EditorTypeNavigationTarget>();

            var navigationTargetsVisitor = new NavigationTargetsVisitor(snapshot, _glyphService, cancellationToken);

            await Task.Run(() =>
            {
                var syntaxTree = snapshot.GetSyntaxTree(cancellationToken);
                navigationTargets.AddRange(navigationTargetsVisitor.GetTargets((CompilationUnitSyntax) syntaxTree.Root));
            }, cancellationToken);

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