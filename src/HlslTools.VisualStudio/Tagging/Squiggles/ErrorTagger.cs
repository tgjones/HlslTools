using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HlslTools.Diagnostics;
using HlslTools.VisualStudio.ErrorList;
using HlslTools.VisualStudio.Options;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace HlslTools.VisualStudio.Tagging.Squiggles
{
    internal abstract class ErrorTagger : AsyncTagger<IErrorTag>
    {
        private readonly string _errorType;
        private readonly ITextView _textView;
        private readonly IOptionsService _optionsService;
        private bool _errorReportingEnabled;
        private bool _squigglesEnabled;

        protected IErrorListHelper ErrorListHelper { get; }

        protected ErrorTagger(string errorType, ITextView textView,
            IOptionsService optionsService, IServiceProvider serviceProvider, 
            ITextDocumentFactoryService textDocumentFactoryService)
        {
            _errorType = errorType;
            _textView = textView;
            _optionsService = optionsService;

            optionsService.OptionsChanged += OnOptionsChanged;

            ITextDocument document;
            if (textDocumentFactoryService.TryGetTextDocument(textView.TextBuffer, out document))
                ErrorListHelper = new ErrorListHelper(serviceProvider, document);

            textView.Closed += OnViewClosed;

            OnOptionsChanged(this, EventArgs.Empty);
        }

        private async void OnOptionsChanged(object sender, EventArgs e)
        {
            var options = _optionsService.AdvancedOptions;
            _errorReportingEnabled = options.EnableErrorReporting;
            _squigglesEnabled = options.EnableSquiggles;

            ErrorListHelper?.Clear();

            await InvalidateTags(_textView.TextSnapshot, CancellationToken.None);
        }

        protected ITagSpan<IErrorTag> CreateTagSpan(ITextSnapshot snapshot, Diagnostic diagnostic, bool squigglesEnabled)
        {
            ErrorListHelper?.AddError(diagnostic, diagnostic.Span);

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

            ErrorListHelper?.Dispose();
        }

        protected override Tuple<ITextSnapshot, List<ITagSpan<IErrorTag>>> GetTags(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            if (!_errorReportingEnabled)
                return Tuple.Create(snapshot, new List<ITagSpan<IErrorTag>>());

            var tagSpans = GetDiagnostics(snapshot, cancellationToken)
                .Select(d => CreateTagSpan(snapshot, d, _squigglesEnabled))
                .Where(x => x != null)
                .ToList();
            return Tuple.Create(snapshot, tagSpans);
        }

        protected abstract IEnumerable<Diagnostic> GetDiagnostics(ITextSnapshot snapshot, CancellationToken cancellationToken);
    }
}