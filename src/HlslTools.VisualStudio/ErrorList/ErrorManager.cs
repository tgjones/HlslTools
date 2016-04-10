using System;
using System.Collections.Generic;
using System.Threading;
using HlslTools.Diagnostics;
using HlslTools.VisualStudio.Options;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace HlslTools.VisualStudio.ErrorList
{
    internal abstract class ErrorManager
    {
        private readonly object _lockObject = new object();
        private readonly ITextView _textView;
        private readonly IOptionsService _optionsService;
        private bool _errorReportingEnabled;

        protected IErrorListHelper ErrorListHelper { get; }

        protected ErrorManager(ITextView textView,
            IOptionsService optionsService, IServiceProvider serviceProvider,
            ITextDocumentFactoryService textDocumentFactoryService)
        {
            _textView = textView;
            _optionsService = optionsService;

            optionsService.OptionsChanged += OnOptionsChanged;

            ITextDocument document;
            if (textDocumentFactoryService.TryGetTextDocument(textView.TextBuffer, out document))
                ErrorListHelper = new ErrorListHelper(serviceProvider, document);

            textView.Closed += OnViewClosed;

            OnOptionsChanged(this, EventArgs.Empty);
        }

        private void OnOptionsChanged(object sender, EventArgs e)
        {
            var options = _optionsService.AdvancedOptions;
            _errorReportingEnabled = options.EnableErrorReporting;

            ErrorListHelper?.Clear();

            RefreshErrors(_textView.TextSnapshot, CancellationToken.None);
        }

        private void OnViewClosed(object sender, EventArgs e)
        {
            _optionsService.OptionsChanged -= OnOptionsChanged;

            var view = (IWpfTextView)sender;
            view.Closed -= OnViewClosed;

            ErrorListHelper?.Dispose();
        }

        protected void RefreshErrors(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            if (!_errorReportingEnabled)
                return;

            var diagnostics = GetDiagnostics(snapshot, cancellationToken);

            lock (_lockObject)
            {
                ErrorListHelper?.Clear();
                foreach (var diagnostic in diagnostics)
                    ErrorListHelper?.AddError(diagnostic, diagnostic.Span);
            }
        }

        protected abstract IEnumerable<Diagnostic> GetDiagnostics(ITextSnapshot snapshot, CancellationToken cancellationToken);
    }
}