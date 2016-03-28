using System;
using HlslTools.Diagnostics;
using HlslTools.Text;
using HlslTools.VisualStudio.Navigation;
using HlslTools.VisualStudio.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;

namespace HlslTools.VisualStudio.ErrorList
{
    internal sealed class ErrorListHelper : IErrorListHelper, IDisposable
    {
        private readonly ErrorListProvider _errorListProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITextDocument _textDocument;

        public ErrorListHelper(IServiceProvider serviceProvider, ITextDocument textDocument)
        {
            _errorListProvider = new ErrorListProvider(serviceProvider);
            _serviceProvider = serviceProvider;
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
                ErrorCategory = (diagnostic.Severity == DiagnosticSeverity.Error)
                    ? TaskErrorCategory.Error 
                    : TaskErrorCategory.Warning,
                Priority = TaskPriority.Normal,
                Document = span.Filename ?? _textDocument.FilePath
            };

            task.Navigate += OnTaskNavigate;

            _errorListProvider.Tasks.Add(task);
        }

        private void OnTaskNavigate(object sender, EventArgs e)
        {
            var task = (ErrorTask)sender;
            _serviceProvider.NavigateTo(task.Document, task.Line, task.Column, task.Line, task.Column);
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