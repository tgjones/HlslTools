using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HlslTools.Compilation;
using HlslTools.VisualStudio.IntelliSense.SignatureHelp.SignatureHelpModelProviders;
using HlslTools.VisualStudio.Text;
using HlslTools.VisualStudio.Util;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace HlslTools.VisualStudio.IntelliSense.SignatureHelp
{
    internal sealed class SignatureHelpManager
    {
        private readonly ITextView _textView;
        private readonly ISignatureHelpBroker _signatureHelpBroker;
        private readonly SignatureHelpModelProviderService _signatureHelpModelProviderService;
        private readonly VisualStudioSourceTextFactory _sourceTextFactory;
        private readonly object _selectedItemIndexKey = new object();

        private ISignatureHelpSession _session;
        private SignatureHelpModel _model;

        public SignatureHelpManager(ITextView textView, ISignatureHelpBroker signatureHelpBroker, SignatureHelpModelProviderService signatureHelpModelProviderService, VisualStudioSourceTextFactory sourceTextFactory)
        {
            _textView = textView;
            _textView.Caret.PositionChanged += CaretOnPositionChanged;
            _textView.TextBuffer.PostChanged += TextBufferOnPostChanged;
            _signatureHelpBroker = signatureHelpBroker;
            _signatureHelpModelProviderService = signatureHelpModelProviderService;
            _sourceTextFactory = sourceTextFactory;
        }

        private void SessionOnDismissed(object sender, EventArgs e)
        {
            _session.Dismissed -= SessionOnDismissed;
            _session.SelectedSignatureChanged -= SessionOnDismissed;
            _session = null;
        }

        private void SessionOnSelectedSignatureChanged(object sender, SelectedSignatureChangedEventArgs e)
        {
            var selectedItemIndex = _session.Signatures.IndexOf(e.NewSelectedSignature);

            _session.Properties.RemoveProperty(_selectedItemIndexKey);
            _session.Properties.AddProperty(_selectedItemIndexKey, selectedItemIndex);
        }

        private void CaretOnPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            UpdateSession();
        }

        private void TextBufferOnPostChanged(object sender, EventArgs e)
        {
            UpdateSession();
        }

        private static bool IsTriggerChar(char c)
        {
            return c == '(' ||
                   c == ',';
        }

        public void HandleTextInput(string text)
        {
            if (_session == null && text.Any(IsTriggerChar))
                TriggerSignatureHelp();
        }

        public void TriggerSignatureHelp()
        {
            if (_session != null)
            {
                _session.Dismiss();
            }
            else
            {
                UpdateModel();
            }
        }

        private int? GetSelectedItemIndex()
        {
            if (_session == null)
                return null;

            int selectedIndex;
            if (!_session.Properties.TryGetProperty(_selectedItemIndexKey, out selectedIndex))
                return null;

            return selectedIndex;
        }

        private async Task UpdateModel()
        {
            var selectedIndex = GetSelectedItemIndex();

            var snapshot = _textView.TextSnapshot;
            var triggerPosition = _textView.GetPosition(snapshot);

            SemanticModel semanticModel;
            try
            {
                semanticModel = await Task.Run(() => _textView.TextBuffer.CurrentSnapshot.GetSemanticModel(_sourceTextFactory, CancellationToken.None));
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to get semantic model: " + ex);
                return;
            }

            var model = GetSignatureHelpModel(semanticModel, triggerPosition, _signatureHelpModelProviderService.Providers);

            // If we previously recorded a selected item and the index is still valid,
            // let's restore it.

            if (model != null && selectedIndex != null && selectedIndex < model.Signatures.Length)
            {
                var selectedItem = model.Signatures[selectedIndex.Value];
                if (selectedItem.Parameters.Length > model.SelectedParameter)
                    model = model.WithSignature(selectedItem);
            }

            // Let observers know that we've a new model.

            Model = model;
        }

        private static SignatureHelpModel GetSignatureHelpModel(SemanticModel semanticModel, int position, IEnumerable<ISignatureHelpModelProvider> providers)
        {
            return providers
                .Select(p => p.GetModel(semanticModel, semanticModel.Compilation.SyntaxTree.MapRootFilePosition(position)))
                .FirstOrDefault(t => t != null);
        }

        private void UpdateSession()
        {
            if (_session == null)
                return;

            UpdateModel().ContinueWith(t =>
            {
                _session.Recalculate();
                _session.Match();
            }, CancellationToken.None,
            TaskContinuationOptions.OnlyOnRanToCompletion, 
            TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void OnModelChanged(EventArgs e)
        {
            var handler = ModelChanged;
            if (handler != null)
                handler(this, e);
        }

        public SignatureHelpModel Model
        {
            get { return _model; }
            private set
            {
                _model = value;
                OnModelChanged(EventArgs.Empty);

                var hasData = _model != null && _model.Signatures.Length > 0;
                var showSession = _session == null && hasData;
                var hideSession = _session != null && !hasData;

                if (hideSession)
                {
                    _session.Dismiss();
                }
                else if (showSession)
                {
                    var snapshot = _textView.TextBuffer.CurrentSnapshot;
                    var triggerPosition = _model.ApplicableSpan.Start;
                    var triggerPoint = snapshot.CreateTrackingPoint(triggerPosition, PointTrackingMode.Negative);

                    _session = _signatureHelpBroker.CreateSignatureHelpSession(_textView, triggerPoint, true);
                    _session.Properties.AddProperty(typeof(SignatureHelpManager), this);
                    _session.Dismissed += SessionOnDismissed;
                    _session.SelectedSignatureChanged += SessionOnSelectedSignatureChanged;
                    _session.Start();
                    _session.Match();
                }
            }
        }

        public event EventHandler<EventArgs> ModelChanged;
    }
}