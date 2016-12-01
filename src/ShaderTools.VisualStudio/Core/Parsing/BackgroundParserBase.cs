using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;

namespace ShaderTools.VisualStudio.Core.Parsing
{
    internal abstract class BackgroundParserBase : IDisposable
    {
        public event EventHandler<BackgroundParserEventArgs> SyntaxTreeAvailable;
        public event EventHandler<BackgroundParserEventArgs> SemanticModelAvailable;

        private readonly ITextBuffer _textBuffer;
        private readonly CancellationTokenSource _shutdownToken;

        private readonly object _lockObject = new object();
        private readonly ManualResetEventSlim _hasWork;

        private CancellationTokenSource _currentParseCancellationTokenSource;

        protected BackgroundParserBase(ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;

            _shutdownToken = new CancellationTokenSource();

            _hasWork = new ManualResetEventSlim(false);

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

        private void DoWork()
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
                    var cancellationToken = _currentParseCancellationTokenSource.Token;

                    CreateSyntaxTree(snapshot, cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();

                    var args = new BackgroundParserEventArgs(snapshot, cancellationToken);
                    RaiseEvent(SyntaxTreeAvailable, args);

                    if (TryCreateSemanticModel(snapshot, cancellationToken))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        RaiseEvent(SemanticModelAvailable, args);
                    }
                }
                catch (OperationCanceledException)
                {

                }

                Thread.Yield();
            }
        }

        protected abstract void CreateSyntaxTree(ITextSnapshot snapshot, CancellationToken cancellationToken);

        protected abstract bool TryCreateSemanticModel(ITextSnapshot snapshot, CancellationToken cancellationToken);

        private void RaiseEvent(EventHandler<BackgroundParserEventArgs> handler, BackgroundParserEventArgs args)
        {
            if (handler == null)
                return;

            foreach (EventHandler<BackgroundParserEventArgs> h in handler.GetInvocationList())
            {
                args.CancellationToken.ThrowIfCancellationRequested();
                h(this, args);
            }
        }

        public IDisposable SubscribeToThrottledSyntaxTreeAvailable(BackgroundParserSubscriptionDelay delay, Action<BackgroundParserEventArgs> callback)
        {
            return Observable.FromEventPattern<BackgroundParserEventArgs>(x => SyntaxTreeAvailable += x, x => SyntaxTreeAvailable -= x)
                .Throttle(GetDelay(delay))
                .Subscribe(x => callback(x.EventArgs));
        }

        public IDisposable SubscribeToThrottledSemanticModelAvailable(BackgroundParserSubscriptionDelay delay, Action<BackgroundParserEventArgs> callback)
        {
            return Observable.FromEventPattern<BackgroundParserEventArgs>(x => SemanticModelAvailable += x, x => SemanticModelAvailable -= x)
                .Throttle(GetDelay(delay))
                .Subscribe(x => callback(x.EventArgs));
        }

        private static TimeSpan GetDelay(BackgroundParserSubscriptionDelay delay)
        {
            const int nearImmediateDelay = 50;
            const int shortDelay = 250;
            const int mediumDelay = 500;
            const int idleDelay = 1500;

            switch (delay)
            {
                case BackgroundParserSubscriptionDelay.NearImmediate:
                    return TimeSpan.FromMilliseconds(nearImmediateDelay);
                case BackgroundParserSubscriptionDelay.Short:
                    return TimeSpan.FromMilliseconds(shortDelay);
                case BackgroundParserSubscriptionDelay.Medium:
                    return TimeSpan.FromMilliseconds(mediumDelay);
                case BackgroundParserSubscriptionDelay.OnIdle:
                default:
                    return TimeSpan.FromMilliseconds(idleDelay);
            }
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

    internal enum BackgroundParserSubscriptionDelay
    {
        NearImmediate,
        Short,
        Medium,
        OnIdle
    }

    internal sealed class BackgroundParserEventArgs : EventArgs
    {
        public ITextSnapshot Snapshot { get; }
        public CancellationToken CancellationToken { get; }

        public BackgroundParserEventArgs(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            Snapshot = snapshot;
            CancellationToken = cancellationToken;
        }
    }
}