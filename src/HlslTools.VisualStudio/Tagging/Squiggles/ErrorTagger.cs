using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HlslTools.Diagnostics;
using HlslTools.VisualStudio.ErrorList;
using HlslTools.VisualStudio.Options;
using HlslTools.VisualStudio.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Task = System.Threading.Tasks.Task;

namespace HlslTools.VisualStudio.Tagging.Squiggles
{
    internal abstract class ErrorTagger : AsyncTagger<IErrorTag>, IBackgroundParserSyntaxTreeHandler
    {
        private readonly string _errorType;
        private readonly IOptionsService _optionsService;
        private SnapshotSyntaxTree _latestSnapshotSyntaxTree;
        private bool _errorReportingEnabled;
        private bool _squigglesEnabled;
        private readonly IErrorListHelper _errorListHelper;

        protected ErrorTagger(string errorType, ITextView textView, BackgroundParser backgroundParser,
            IOptionsService optionsService, IServiceProvider serviceProvider, 
            ITextDocumentFactoryService textDocumentFactoryService)
        {
            _errorType = errorType;
            _optionsService = optionsService;

            optionsService.OptionsChanged += OnOptionsChanged;

            ITextDocument document;
            if (textDocumentFactoryService.TryGetTextDocument(textView.TextBuffer, out document))
                _errorListHelper = new ErrorListHelper(serviceProvider, document);

            textView.Closed += OnViewClosed;

            backgroundParser.RegisterSyntaxTreeHandler(BackgroundParserHandlerPriority.Low, this);

            OnOptionsChanged(this, EventArgs.Empty);
        }

        private void OnOptionsChanged(object sender, EventArgs e)
        {
            var options = _optionsService.AdvancedOptions;
            _errorReportingEnabled = options.EnableErrorReporting;
            _squigglesEnabled = options.EnableSquiggles;

            _errorListHelper?.Clear();

            if (_latestSnapshotSyntaxTree == null)
                return;

            InvalidateTags(_latestSnapshotSyntaxTree, CancellationToken.None);
        }

        protected ITagSpan<IErrorTag> CreateTagSpan(ITextSnapshot snapshot, Diagnostic diagnostic, bool squigglesEnabled)
        {
            _errorListHelper?.AddError(diagnostic, diagnostic.Span);

            if (!diagnostic.Span.IsInRootFile || !squigglesEnabled)
                return null;

            var span = new Span(diagnostic.Span.Start, diagnostic.Span.Length);
            var snapshotSpan = new SnapshotSpan(snapshot, span);
            var errorTag = new ErrorTag(_errorType, diagnostic.Message);
            var errorTagSpan = new TagSpan<IErrorTag>(snapshotSpan, errorTag);

            return errorTagSpan;
        }

        private void OnViewClosed(object sender, EventArgs e)
        {
            _optionsService.OptionsChanged -= OnOptionsChanged;

            var view = (IWpfTextView)sender;
            view.Closed -= OnViewClosed;

            _errorListHelper?.Dispose();
        }

        public override Task InvalidateTags(SnapshotSyntaxTree snapshotSyntaxTree, CancellationToken cancellationToken)
        {
            _latestSnapshotSyntaxTree = snapshotSyntaxTree;
            return base.InvalidateTags(snapshotSyntaxTree, cancellationToken);
        }

        async Task IBackgroundParserSyntaxTreeHandler.OnSyntaxTreeAvailable(SnapshotSyntaxTree snapshotSyntaxTree, CancellationToken cancellationToken)
        {
            _errorListHelper.Clear();

            await InvalidateTags(snapshotSyntaxTree, cancellationToken);
        }

        protected override Tuple<ITextSnapshot, List<ITagSpan<IErrorTag>>> GetTags(SnapshotSyntaxTree snapshotSyntaxTree, CancellationToken cancellationToken)
        {
            if (!_errorReportingEnabled)
                return Tuple.Create(snapshotSyntaxTree.Snapshot, new List<ITagSpan<IErrorTag>>());

            var snapshot = snapshotSyntaxTree.Snapshot;
            var tagSpans = GetDiagnostics(snapshotSyntaxTree)
                .Select(d => CreateTagSpan(snapshot, d, _squigglesEnabled))
                .Where(x => x != null)
                .ToList();
            return Tuple.Create(snapshot, tagSpans);
        }

        protected abstract IEnumerable<Diagnostic> GetDiagnostics(SnapshotSyntaxTree snapshotSyntaxTree);
    }
}