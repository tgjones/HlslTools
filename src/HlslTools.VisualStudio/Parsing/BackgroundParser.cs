using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HlslTools.Compilation;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Text;

namespace HlslTools.VisualStudio.Parsing
{
    internal sealed class BackgroundParser : IDisposable
    {
        private readonly ITextBuffer _textBuffer;
        private readonly CancellationTokenSource _shutdownToken;

        private readonly object _lockObject = new object();
        private readonly ManualResetEventSlim _hasWork;

        private CancellationTokenSource _currentParseCancellationTokenSource;

        private readonly SortedList<BackgroundParserHandlerPriority, List<IBackgroundParserSyntaxTreeHandler>> _syntaxTreeAvailableEventHandlers;
        private readonly SortedList<BackgroundParserHandlerPriority, List<IBackgroundParserSemanticModelHandler>> _semanticModelAvailableEventHandlers;

        public BackgroundParser(ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;

            _shutdownToken = new CancellationTokenSource();

            _hasWork = new ManualResetEventSlim(false);

            _syntaxTreeAvailableEventHandlers = new SortedList<BackgroundParserHandlerPriority, List<IBackgroundParserSyntaxTreeHandler>>();
            _semanticModelAvailableEventHandlers = new SortedList<BackgroundParserHandlerPriority, List<IBackgroundParserSemanticModelHandler>>();

            _textBuffer.ChangedHighPriority += OnTextBufferChanged;

            Task.Run(() => DoWork(), _shutdownToken.Token);
        }

        private void OnTextBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            lock (_lockObject)
            {
                _currentParseCancellationTokenSource?.Cancel();
                _hasWork.Set();
            }
        }

        private async void DoWork()
        {
            while (!_shutdownToken.IsCancellationRequested)
            {
                try
                {
                    _hasWork.Wait(_shutdownToken.Token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                _hasWork.Reset();

                lock (_lockObject)
                {
                    _currentParseCancellationTokenSource = new CancellationTokenSource();
                }

                var snapshot = _textBuffer.CurrentSnapshot;

                try
                {
                    using (var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_shutdownToken.Token, _currentParseCancellationTokenSource.Token))
                    {
                        var cancellationToken = cancellationTokenSource.Token;

                        // Force creation of SyntaxTree.
                        snapshot.GetSyntaxTree(cancellationToken);

                        cancellationToken.ThrowIfCancellationRequested();

                        await RaiseSyntaxTreeAvailable(snapshot, cancellationToken);

                        // Force creation of SemanticModel.
                        SemanticModel semanticModel;
                        if (snapshot.TryGetSemanticModel(cancellationToken, out semanticModel))
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            await RaiseSemanticModelAvailable(snapshot, cancellationToken);
                        }
                    }
                }
                catch (OperationCanceledException)
                {

                }

                Thread.Yield();
            }
        }

        private async Task RaiseSyntaxTreeAvailable(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            foreach (var handlerList in _syntaxTreeAvailableEventHandlers)
                foreach (var handler in handlerList.Value)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await handler.OnSyntaxTreeAvailable(snapshot, cancellationToken);
                }
        }

        private async Task RaiseSemanticModelAvailable(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            foreach (var handlerList in _semanticModelAvailableEventHandlers)
                foreach (var handler in handlerList.Value)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await handler.OnSemanticModelAvailable(snapshot, cancellationToken);
                }
        }

        public void RegisterSyntaxTreeHandler(BackgroundParserHandlerPriority priority, IBackgroundParserSyntaxTreeHandler handler)
        {
            List<IBackgroundParserSyntaxTreeHandler> handlerList;
            if (!_syntaxTreeAvailableEventHandlers.TryGetValue(priority, out handlerList))
                _syntaxTreeAvailableEventHandlers.Add(priority, handlerList = new List<IBackgroundParserSyntaxTreeHandler>());
            handlerList.Add(handler);
        }

        public void RegisterSemanticModelHandler(BackgroundParserHandlerPriority priority, IBackgroundParserSemanticModelHandler handler)
        {
            List<IBackgroundParserSemanticModelHandler> handlerList;
            if (!_semanticModelAvailableEventHandlers.TryGetValue(priority, out handlerList))
                _semanticModelAvailableEventHandlers.Add(priority, handlerList = new List<IBackgroundParserSemanticModelHandler>());
            handlerList.Add(handler);
        }

        public void Dispose()
        {
            if (_currentParseCancellationTokenSource != null)
            {
                _currentParseCancellationTokenSource.Cancel();
                _currentParseCancellationTokenSource.Dispose();
                _currentParseCancellationTokenSource = null;
            }
            _shutdownToken.Cancel();
            _shutdownToken.Dispose();
            _hasWork.Dispose();
        }
    }

    internal enum BackgroundParserHandlerPriority
    {
        Lowest,
        Low,
        Medium,
        High,
        Highest
    }

    internal interface IBackgroundParserSyntaxTreeHandler
    {
        Task OnSyntaxTreeAvailable(ITextSnapshot snapshot, CancellationToken cancellationToken);
    }

    internal interface IBackgroundParserSemanticModelHandler
    {
        Task OnSemanticModelAvailable(ITextSnapshot snapshot, CancellationToken cancellationToken);
    }
}