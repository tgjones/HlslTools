using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HlslTools.Compilation;
using HlslTools.VisualStudio.IntelliSense.QuickInfo.QuickInfoModelProviders;
using HlslTools.VisualStudio.Text;
using HlslTools.VisualStudio.Util;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace HlslTools.VisualStudio.IntelliSense.QuickInfo
{
    internal sealed class QuickInfoManager
    {
        private readonly ITextView _textView;
        private readonly IQuickInfoBroker _quickInfoBroker;
        private readonly QuickInfoModelProviderService _quickInfoModelProviderService;
        private readonly VisualStudioSourceTextFactory _sourceTextFactory;

        private QuickInfoModel _model;
        private IQuickInfoSession _session;

        public QuickInfoManager(ITextView textView, IQuickInfoBroker quickInfoBroker, QuickInfoModelProviderService quickInfoModelProviderService, VisualStudioSourceTextFactory sourceTextFactory)
        {
            _textView = textView;
            _quickInfoBroker = quickInfoBroker;
            _quickInfoModelProviderService = quickInfoModelProviderService;
            _sourceTextFactory = sourceTextFactory;
        }

        public async void TriggerQuickInfo(int offset)
        {
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

            Model = GetQuickInfoModel(semanticModel, offset, _quickInfoModelProviderService.Providers);
        }

        private static QuickInfoModel GetQuickInfoModel(SemanticModel semanticModel, int position, IEnumerable<IQuickInfoModelProvider> providers)
        {
            return providers
                .Select(p => p.GetModel(semanticModel, semanticModel.Compilation.SyntaxTree.MapRootFilePosition(position)))
                .FirstOrDefault(t => t != null);
        }

        private void OnModelChanged(EventArgs e)
        {
            ModelChanged?.Invoke(this, e);
        }

        public QuickInfoModel Model
        {
            get { return _model; }
            private set
            {
                if (_model != value)
                {
                    _model = value;
                    OnModelChanged(EventArgs.Empty);

                    var hasData = _model != null;
                    var showSession = _session == null && hasData;
                    var hideSession = _session != null && !hasData;

                    if (hideSession)
                    {
                        _session.Dismiss();
                    }
                    else if (showSession)
                    {
                        var syntaxTree = _model.SemanticModel.Compilation.SyntaxTree;
                        var snapshot = syntaxTree.Text.ToTextSnapshot();
                        var triggerPosition = _model.Span.Start;
                        var triggerPoint = snapshot.CreateTrackingPoint(triggerPosition, PointTrackingMode.Negative);

                        _session = _quickInfoBroker.CreateQuickInfoSession(_textView, triggerPoint, true);
                        _session.Properties.AddProperty(typeof(QuickInfoManager), this);
                        _session.Dismissed += SessionOnDismissed;
                        _session.Start();
                    }
                }
            }
        }

        private void SessionOnDismissed(object sender, EventArgs e)
        {
            _session = null;
        }

        public event EventHandler<EventArgs> ModelChanged;
    }
}