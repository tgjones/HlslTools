using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.CodeAnalysis.Classification;
using ShaderTools.CodeAnalysis.Editor.Shared.Extensions;
using ShaderTools.CodeAnalysis.Editor.Shared.Tagging;
using ShaderTools.CodeAnalysis.Editor.Shared.Utilities;
using ShaderTools.CodeAnalysis.Editor.Tagging;
using ShaderTools.CodeAnalysis.Notification;
using ShaderTools.CodeAnalysis.Shared.TestHooks;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.Classification
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType(ContentTypeNames.ShaderToolsContentType)]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [TagType(typeof(IClassificationTag))]
    internal sealed class SemanticClassificationTaggerProvider : AsynchronousViewTaggerProvider<IClassificationTag>
    {
        private readonly ClassificationTypeMap _typeMap;
        private readonly ISemanticChangeNotificationService _semanticChangeNotificationService;

        [ImportingConstructor]
        public SemanticClassificationTaggerProvider(
            ClassificationTypeMap typeMap,
            ISemanticChangeNotificationService semanticChangeNotificationService,
            IForegroundNotificationService notificationService,
            [ImportMany] IEnumerable<Lazy<IAsynchronousOperationListener, FeatureMetadata>> asyncListeners) 
            : base(new AggregateAsynchronousOperationListener(asyncListeners, FeatureAttribute.KeywordHighlighting), notificationService)
        {
            _typeMap = typeMap;
            _semanticChangeNotificationService = semanticChangeNotificationService;
        }

        protected override ITaggerEventSource CreateEventSource(ITextView textView, ITextBuffer subjectBuffer)
        {
            this.AssertIsForeground();
            const TaggerDelay Delay = TaggerDelay.Short;

            // Note: we don't listen for OnTextChanged.  They'll get reported by the ViewSpan changing 
            // and also the SemanticChange nodification. 
            // 
            // Note: when the user scrolls, we will try to reclassify as soon as possible.  That way
            // we appear semantically unclassified for a very short amount of time.
            return TaggerEventSources.Compose(
                TaggerEventSources.OnViewSpanChanged(textView, textChangeDelay: Delay, scrollChangeDelay: TaggerDelay.NearImmediate),
                TaggerEventSources.OnSemanticChanged(subjectBuffer, Delay, _semanticChangeNotificationService));
        }

        protected override IEnumerable<SnapshotSpan> GetSpansToTag(ITextView textView, ITextBuffer subjectBuffer)
        {
            this.AssertIsForeground();

            // Find the visible span some 100 lines +/- what's actually in view.  This way
            // if the user scrolls up/down, we'll already have the results.
            var visibleSpanOpt = textView.GetVisibleLinesSpan(subjectBuffer, extraLines: 100);
            if (visibleSpanOpt == null)
            {
                // Couldn't find anything visible, just fall back to classifying everything.
                return base.GetSpansToTag(textView, subjectBuffer);
            }

            return new[] { visibleSpanOpt.Value };
        }

        protected override async Task ProduceTagsAsync(TaggerContext<IClassificationTag> context, DocumentSnapshotSpan spanToTag, int? caretPosition)
        {
            var document = spanToTag.Document;
            var classificationService = document?.LanguageServices.GetService<IClassificationService>();

            if (classificationService == null)
            {
                return;
            }

            Workspace.TryGetWorkspace(document.SourceText.Container, out var workspace);

            var semanticModel = await document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            var classifiedSpans = ClassificationUtilities.GetOrCreateClassifiedSpanList();

            classificationService.AddSemanticClassifications(
                semanticModel, 
                spanToTag.SnapshotSpan.Span.ToTextSpan(),
                workspace,
                classifiedSpans, 
                context.CancellationToken);

            ClassificationUtilities.Convert(_typeMap, spanToTag.SnapshotSpan.Snapshot, classifiedSpans, context.AddTag);
            ClassificationUtilities.ReturnClassifiedSpanList(classifiedSpans);
        }
    }
}
