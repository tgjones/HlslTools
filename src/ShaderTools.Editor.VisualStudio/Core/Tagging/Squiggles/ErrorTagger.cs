using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Editor.VisualStudio.Core.Options;

namespace ShaderTools.Editor.VisualStudio.Core.Tagging.Squiggles
{
    internal abstract class ErrorTagger : AsyncTagger<IErrorTag>
    {
        private readonly string _errorType;
        private readonly ITextBuffer _textBuffer;
        private readonly IOptionsService _optionsService;
        private bool _squigglesEnabled;

        protected ErrorTagger(string errorType, ITextView textView, ITextBuffer textBuffer, IOptionsService optionsService)
        {
            _errorType = errorType;
            _textBuffer = textBuffer;
            _optionsService = optionsService;

            optionsService.OptionsChanged += OnOptionsChanged;

            textView.Closed += OnViewClosed;

            OnOptionsChanged(this, EventArgs.Empty);
        }

        private async void OnOptionsChanged(object sender, EventArgs e)
        {
            _squigglesEnabled = _optionsService.EnableErrorReporting && _optionsService.EnableSquiggles;

            await InvalidateTags(_textBuffer.CurrentSnapshot, CancellationToken.None);
        }

        private ITagSpan<IErrorTag> CreateTagSpan(ITextSnapshot snapshot, TextSpan textSpan, Diagnostic diagnostic, bool squigglesEnabled)
        {
            var span = new Span(textSpan.Start, textSpan.Length);
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

            var diagnostics = GetDiagnostics(snapshot, cancellationToken);
            var tagSpans = diagnostics.Item2
                .Select(x => new { Diagnostic = x, Span = diagnostics.Item1.GetSourceFileSpan(x.SourceRange) })
                .Where(x => x.Span.IsInRootFile)
                .Select(d => CreateTagSpan(snapshot, d.Span.Span, d.Diagnostic, _squigglesEnabled))
                .Where(x => x != null)
                .ToList();
            return Tuple.Create(snapshot, tagSpans);
        }

        protected abstract Tuple<SyntaxTreeBase, IEnumerable<Diagnostic>> GetDiagnostics(ITextSnapshot snapshot, CancellationToken cancellationToken);
    }
}