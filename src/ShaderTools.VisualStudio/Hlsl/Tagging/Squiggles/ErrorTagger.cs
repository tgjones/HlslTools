using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using ShaderTools.Hlsl.Diagnostics;
using ShaderTools.VisualStudio.Core.Tagging;
using ShaderTools.VisualStudio.Hlsl.Options;

namespace ShaderTools.VisualStudio.Hlsl.Tagging.Squiggles
{
    internal abstract class ErrorTagger : AsyncTagger<IErrorTag>
    {
        private readonly string _errorType;
        private readonly ITextView _textView;
        private readonly IOptionsService _optionsService;
        private bool _squigglesEnabled;

        protected ErrorTagger(string errorType, ITextView textView,
            IOptionsService optionsService)
        {
            _errorType = errorType;
            _textView = textView;
            _optionsService = optionsService;

            optionsService.OptionsChanged += OnOptionsChanged;

            textView.Closed += OnViewClosed;

            OnOptionsChanged(this, EventArgs.Empty);
        }

        private async void OnOptionsChanged(object sender, EventArgs e)
        {
            var options = _optionsService.AdvancedOptions;
            _squigglesEnabled = options.EnableErrorReporting && options.EnableSquiggles;

            await InvalidateTags(_textView.TextSnapshot, CancellationToken.None);
        }

        protected ITagSpan<IErrorTag> CreateTagSpan(ITextSnapshot snapshot, Diagnostic diagnostic, bool squigglesEnabled)
        {
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
        }

        protected override Tuple<ITextSnapshot, List<ITagSpan<IErrorTag>>> GetTags(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            if (!_squigglesEnabled)
                return Tuple.Create(snapshot, new List<ITagSpan<IErrorTag>>());

            var tagSpans = GetDiagnostics(snapshot, cancellationToken)
                .Where(x => x.Span.IsInRootFile)
                .Select(d => CreateTagSpan(snapshot, d, _squigglesEnabled))
                .Where(x => x != null)
                .ToList();
            return Tuple.Create(snapshot, tagSpans);
        }

        protected abstract IEnumerable<Diagnostic> GetDiagnostics(ITextSnapshot snapshot, CancellationToken cancellationToken);
    }
}