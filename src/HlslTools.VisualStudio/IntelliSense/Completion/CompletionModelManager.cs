using System;
using System.Threading;
using System.Threading.Tasks;
using HlslTools.Compilation;
using HlslTools.VisualStudio.IntelliSense.Completion.CompletionProviders;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace HlslTools.VisualStudio.IntelliSense.Completion
{
    internal sealed class CompletionModelManager
    {
        private readonly ITextView _textView;
        private readonly ICompletionBroker _completionBroker;
        private readonly CompletionProviderService _completionProviderService;

        private ICompletionSession _session;
        private CompletionModel _model;

        public CompletionModelManager(ITextView textView, ICompletionBroker completionBroker, CompletionProviderService completionProviderService)
        {
            _textView = textView;
            _textView.TextBuffer.PostChanged += OnTextBufferOnPostChanged;
            _completionBroker = completionBroker;
            _completionProviderService = completionProviderService;
        }

        public void HandleTextInput(bool isTrigger)
        {
            if (_session == null && isTrigger)
                TriggerCompletion(false);
            else if (_session != null)
                UpdateModel();
        }

        private void OnTextBufferOnPostChanged(object sender, EventArgs e)
        {
            if (_session != null)
                UpdateModel();
        }

        public void TriggerCompletion(bool autoComplete)
        {
            if (_session != null)
                _session.Dismiss();
            else
                UpdateModel();
        }

        private async void UpdateModel()
        {
            var snapshot = _textView.TextSnapshot;
            var triggerPosition = _textView.GetPosition(snapshot);

            SemanticModel semanticModel = null;
            if (!await Task.Run(() => snapshot.TryGetSemanticModel(CancellationToken.None, out semanticModel)))
                return;

            var model = semanticModel.GetCompletionModel(triggerPosition, snapshot, _completionProviderService.Providers);

            // Let observers know that we've a new model.

            Model = model;
        }

        public bool Commit()
        {
            if (_session == null)
                return false;

            var isBuilder = _session.SelectedCompletionSet?.SelectionStatus?.Completion != null 
                && _session.SelectedCompletionSet.CompletionBuilders != null
                && _session.SelectedCompletionSet.CompletionBuilders.Contains(_session.SelectedCompletionSet.SelectionStatus.Completion);

            // If it's a builder, we don't want to eat the enter key.
            var sendThrough = isBuilder;

            _textView.TextBuffer.PostChanged -= OnTextBufferOnPostChanged;
            _session.Commit();
            _textView.TextBuffer.PostChanged += OnTextBufferOnPostChanged;

            return !sendThrough;
        }

        private void SessionOnDismissed(object sender, EventArgs e)
        {
            _session = null;
        }

        private void OnModelChanged(EventArgs e)
        {
            ModelChanged?.Invoke(this, e);
        }

        public CompletionModel Model
        {
            get { return _model; }
            private set
            {
                _model = value;
                OnModelChanged(EventArgs.Empty);

                var hasData = _model != null && _model.Items.Length > 0;
                var showSession = _session == null && hasData;
                var hideSession = _session != null && !hasData;

                if (hideSession)
                {
                    _session.Dismiss();
                }
                else if (showSession)
                {
                    var snapshot = _model.TextSnapshot;
                    var triggerPosition = _model.ApplicableSpan.Start;
                    var triggerPoint = snapshot.CreateTrackingPoint(triggerPosition, PointTrackingMode.Negative);

                    _session = _completionBroker.CreateCompletionSession(_textView, triggerPoint, true);
                    _session.Properties.AddProperty(typeof(CompletionModelManager), this);
                    _session.Dismissed += SessionOnDismissed;
                    _session.Start();
                }
            }
        }

        public event EventHandler<EventArgs> ModelChanged;
    }
}