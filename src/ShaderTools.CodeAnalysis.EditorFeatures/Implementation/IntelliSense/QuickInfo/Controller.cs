// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using ShaderTools.CodeAnalysis.Editor.Commands;
using ShaderTools.CodeAnalysis.Editor.Shared.Extensions;
using ShaderTools.CodeAnalysis.QuickInfo;
using ShaderTools.CodeAnalysis.Shared.TestHooks;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Utilities.ErrorReporting;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.IntelliSense.QuickInfo
{
    internal partial class Controller :
        AbstractController<Session<Controller, Model, IQuickInfoPresenterSession>, Model, IQuickInfoPresenterSession, IQuickInfoSession>,
        ICommandHandler<InvokeQuickInfoCommandArgs>
    {
        private static readonly object s_quickInfoPropertyKey = new object();

        private readonly IQuickInfoProviderCoordinatorFactory _providerCoordinatorFactory;

        public Controller(
            ITextView textView,
            ITextBuffer subjectBuffer,
            IIntelliSensePresenter<IQuickInfoPresenterSession, IQuickInfoSession> presenter,
            IAsynchronousOperationListener asyncListener,
            IDocumentProvider documentProvider,
            IQuickInfoProviderCoordinatorFactory providerCoordinatorFactory)
            : base(textView, subjectBuffer, presenter, asyncListener, documentProvider, "QuickInfo")
        {
            _providerCoordinatorFactory = providerCoordinatorFactory;
        }

        internal static Controller GetInstance(
            CommandArgs args,
            IIntelliSensePresenter<IQuickInfoPresenterSession, IQuickInfoSession> presenter,
            IAsynchronousOperationListener asyncListener,
            IQuickInfoProviderCoordinatorFactory providerCoordinatorFactory)
        {
            var textView = args.TextView;
            var subjectBuffer = args.SubjectBuffer;
            return textView.GetOrCreatePerSubjectBufferProperty(subjectBuffer, s_quickInfoPropertyKey,
                (v, b) => new Controller(v, b,
                    presenter,
                    asyncListener,
                    new DocumentProvider(),
                    providerCoordinatorFactory));
        }

        internal override void OnModelUpdated(Model modelOpt)
        {
            AssertIsForeground();
            if (modelOpt == null || modelOpt.TextVersion != this.SubjectBuffer.CurrentSnapshot.Version)
            {
                this.StopModelComputation();
            }
            else
            {
                // We want the span to actually only go up to the caret.  So get the expected span
                // and then update its end point accordingly.
                ITrackingSpan trackingSpan = null;
                QuickInfoItem item = null;

                // Whether or not we have an item to show, we need to start the session.
                // If the user Edit.QuickInfo's on a squiggle, they want to see the 
                // error text even if there's no symbol quickinfo.
                if (modelOpt.Item != null)
                {
                    item = modelOpt.Item;
                    var triggerSpan = modelOpt.GetCurrentSpanInSnapshot(item.TextSpan, this.SubjectBuffer.CurrentSnapshot);
                    trackingSpan = triggerSpan.CreateTrackingSpan(SpanTrackingMode.EdgeInclusive);
                }
                else
                {
                    var caret = this.TextView.GetCaretPoint(this.SubjectBuffer);
                    if (caret != null)
                        trackingSpan = caret.Value.Snapshot.CreateTrackingSpan(caret.Value.Position, 0, SpanTrackingMode.EdgeInclusive, TrackingFidelityMode.Forward);
                }
                if (trackingSpan != null)
                    sessionOpt.PresenterSession.PresentItem(trackingSpan, item, modelOpt.TrackMouse);
            }
        }

        public void StartSession(
            int position,
            bool trackMouse,
            IQuickInfoSession augmentSession = null)
        {
            AssertIsForeground();

            var providerCoordinator = GetProviderCoordinator();
            if (providerCoordinator == null)
            {
                return;
            }

            var snapshot = this.SubjectBuffer.CurrentSnapshot;
            this.sessionOpt = new Session<Controller, Model, IQuickInfoPresenterSession>(this, new ModelComputation<Model>(this, TaskScheduler.Default),
                this.Presenter.CreateSession(this.TextView, this.SubjectBuffer, augmentSession));

            this.sessionOpt.Computation.ChainTaskAndNotifyControllerWhenFinished(
                (model, cancellationToken) => ComputeModelInBackgroundAsync(position, snapshot, providerCoordinator, trackMouse, cancellationToken));
        }

        private IQuickInfoProviderCoordinator GetProviderCoordinator()
        {
            this.AssertIsForeground();

            var snapshot = this.SubjectBuffer.CurrentSnapshot;
            var document = snapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (document != null)
            {
                return _providerCoordinatorFactory.CreateCoordinator(document);
            }

            return null;
        }

        private async Task<Model> ComputeModelInBackgroundAsync(
               int position,
               ITextSnapshot snapshot,
               IQuickInfoProviderCoordinator providerCoordinator,
               bool trackMouse,
               CancellationToken cancellationToken)
        {
            try
            {
                AssertIsBackground();

                //using (Logger.LogBlock(FunctionId.QuickInfo_ModelComputation_ComputeModelInBackground, cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var document = await DocumentProvider.GetDocumentAsync(snapshot, cancellationToken).ConfigureAwait(false);
                    if (document == null)
                    {
                        return null;
                    }

                    var (item, provider) = await providerCoordinator.GetItemAsync(document, position, cancellationToken).ConfigureAwait(false);
                    return new Model(snapshot.Version, item, provider, trackMouse);
                }
            }
            catch (Exception e) when (FatalError.ReportUnlessCanceled(e))
            {
                throw ExceptionUtilities.Unreachable;
            }
        }
    }
}
