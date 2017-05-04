using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Threading;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Editor.VisualStudio.Core.Navigation;

namespace ShaderTools.Editor.VisualStudio.Core.ErrorList
{
    internal sealed class ErrorListHelper : IErrorListHelper
    {
        private readonly ErrorListProvider _errorListProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITextDocument _textDocument;
        private bool _disposed;

        public ErrorListHelper(IServiceProvider serviceProvider, ITextDocument textDocument)
        {
            _errorListProvider = new ErrorListProvider(serviceProvider);
            _serviceProvider = serviceProvider;
            _textDocument = textDocument;
        }

        public void AddError(SyntaxTreeBase syntaxTree, Diagnostic diagnostic)
        {
            ThreadHelper.JoinableTaskFactory.Run(() =>
            {
                if (_disposed)
                    return TplExtensions.CompletedTask;

                var snapshot = syntaxTree.Text.FindCorrespondingEditorTextSnapshot();
                if (snapshot == null)
                    return TplExtensions.CompletedTask;

                var span = syntaxTree.GetSourceFileSpan(diagnostic.SourceRange);
                var line = snapshot.GetLineFromPosition(span.Span.Start);

                var task = new ErrorTask
                {
                    Text = diagnostic.Message,
                    Line = line.LineNumber,
                    Column = span.Span.Start - line.Start.Position,
                    Category = TaskCategory.CodeSense,
                    ErrorCategory = (diagnostic.Severity == DiagnosticSeverity.Error)
                        ? TaskErrorCategory.Error
                        : TaskErrorCategory.Warning,
                    Priority = TaskPriority.Normal,
                    Document = span.File.FilePath ?? _textDocument.FilePath
                };

                task.Navigate += OnTaskNavigate;

                _errorListProvider.Tasks.Add(task);

                return TplExtensions.CompletedTask;
            });
        }

        private void OnTaskNavigate(object sender, EventArgs e)
        {
            var task = (ErrorTask)sender;
            _serviceProvider.NavigateTo(task.Document, task.Line, task.Column, task.Line, task.Column);
        }

        public void Clear()
        {
            ThreadHelper.JoinableTaskFactory.Run(() =>
            {
                if (_disposed)
                    return TplExtensions.CompletedTask;
                _errorListProvider.Tasks.Clear();
                return TplExtensions.CompletedTask;
            });
        }

        void IDisposable.Dispose()
        {
            ThreadHelper.JoinableTaskFactory.Run(() =>
            {
                if (_errorListProvider != null)
                {
                    _errorListProvider.Tasks.Clear();
                    _errorListProvider.Dispose();
                }
                _disposed = true;
                return TplExtensions.CompletedTask;
            });
        }
    }
}