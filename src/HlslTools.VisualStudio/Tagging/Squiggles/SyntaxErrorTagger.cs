using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HlslTools.VisualStudio.ErrorList;
using HlslTools.VisualStudio.Options;
using HlslTools.VisualStudio.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace HlslTools.VisualStudio.Tagging.Squiggles
{
    internal sealed class SyntaxErrorTagger : ErrorTagger, IBackgroundParserSyntaxTreeHandler
    {
        private readonly IErrorListHelper _errorListHelper;
        private readonly IOptionsService _optionsService;
        private SnapshotSyntaxTree _latestSnapshotSyntaxTree;
        private bool _errorReportingEnabled;
        private bool _squigglesEnabled;

        public SyntaxErrorTagger(ITextView textView, BackgroundParser backgroundParser, IErrorListHelper errorListHelper, IOptionsService optionsService)
            : base(PredefinedErrorTypeNames.SyntaxError, errorListHelper)
        {
            optionsService.OptionsChanged += OnOptionsChanged;
            textView.Closed += (sender, e) => optionsService.OptionsChanged -= OnOptionsChanged;

            _errorListHelper = errorListHelper;
            _optionsService = optionsService;
            backgroundParser.RegisterSyntaxTreeHandler(BackgroundParserHandlerPriority.Low, this);

            OnOptionsChanged(this, EventArgs.Empty);
        }

        private void OnOptionsChanged(object sender, EventArgs e)
        {
            var options = _optionsService.AdvancedOptions;
            _errorReportingEnabled = options.EnableErrorReporting;
            _squigglesEnabled = options.EnableSquiggles;

            // TODO: Only safe to clear error list if each tagger has its own ErrorListHelper.
            _errorListHelper.Clear();

            if (_latestSnapshotSyntaxTree == null)
                return;

            InvalidateTags(_latestSnapshotSyntaxTree, CancellationToken.None);
        }

        protected override Tuple<ITextSnapshot, List<ITagSpan<IErrorTag>>> GetTags(SnapshotSyntaxTree snapshotSyntaxTree, CancellationToken cancellationToken)
        {
            if (!_errorReportingEnabled)
                return Tuple.Create(snapshotSyntaxTree.Snapshot, new List<ITagSpan<IErrorTag>>());

            var syntaxTree = snapshotSyntaxTree.SyntaxTree;
            var snapshot = snapshotSyntaxTree.Snapshot;
            var tagSpans = syntaxTree.GetDiagnostics()
                .Select(d => CreateTagSpan(snapshot, d, _squigglesEnabled))
                .Where(x => x != null)
                .ToList();
            return Tuple.Create(snapshot, tagSpans);
        }

        public override Task InvalidateTags(SnapshotSyntaxTree snapshotSyntaxTree, CancellationToken cancellationToken)
        {
            _latestSnapshotSyntaxTree = snapshotSyntaxTree;
            return base.InvalidateTags(snapshotSyntaxTree, cancellationToken);
        }

        async Task IBackgroundParserSyntaxTreeHandler.OnSyntaxTreeAvailable(SnapshotSyntaxTree snapshotSyntaxTree, CancellationToken cancellationToken)
        {
            // TODO: Only safe to clear error list if each tagger has its own ErrorListHelper.
            _errorListHelper.Clear();

            await InvalidateTags(snapshotSyntaxTree, cancellationToken);
        }
    }
}