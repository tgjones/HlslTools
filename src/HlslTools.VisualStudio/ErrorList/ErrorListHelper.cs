using System;
using System.Diagnostics;
using HlslTools.Diagnostics;
using HlslTools.Text;
using HlslTools.VisualStudio.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace HlslTools.VisualStudio.ErrorList
{
    internal sealed class ErrorListHelper : IErrorListHelper, IDisposable
    {
        private readonly ErrorListProvider _errorListProvider;
        private readonly ITextView _textView;
        private readonly ITextDocument _textDocument;

        public ErrorListHelper(IServiceProvider serviceProvider, ITextView textView, ITextDocument textDocument)
        {
            _errorListProvider = new ErrorListProvider(serviceProvider);
            _textView = textView;
            _textDocument = textDocument;
        }

        public void AddError(Diagnostic diagnostic, TextSpan span)
        {
            var sourceText = span.SourceText as VisualStudioSourceText;
            if (sourceText == null)
                return;

            var line = sourceText.Snapshot.GetLineFromPosition(span.Start);

            var task = new ErrorTask
            {
                Text = diagnostic.Message,
                Line = line.LineNumber,
                Column = span.Start - line.Start.Position,
                Category = TaskCategory.CodeSense,
                ErrorCategory = TaskErrorCategory.Error,
                Priority = TaskPriority.Normal,
                Document = span.Filename ?? _textDocument.FilePath
            };

            task.Navigate += OnTaskNavigate;

            _errorListProvider.Tasks.Add(task);
        }

        private void OnTaskNavigate(object sender, EventArgs e)
        {
            var task = (ErrorTask)sender;
            _errorListProvider.Navigate(task, new Guid("{00000000-0000-0000-0000-000000000000}"));

            var line = _textView.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(task.Line);
            var point = new SnapshotPoint(line.Snapshot, line.Start.Position + task.Column);
            _textView.Caret.MoveTo(point);
        }

        public void Clear()
        {
            _errorListProvider.Tasks.Clear();
        }

        public void Dispose()
        {
            if (_errorListProvider != null)
            {
                _errorListProvider.Tasks.Clear();
                _errorListProvider.Dispose();
            }
        }
    }
}