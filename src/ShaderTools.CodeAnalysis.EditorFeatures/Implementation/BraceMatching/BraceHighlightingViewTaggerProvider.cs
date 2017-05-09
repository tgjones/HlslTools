using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.CodeAnalysis.BraceMatching;
using ShaderTools.CodeAnalysis.Editor.Shared.Tagging;
using ShaderTools.CodeAnalysis.Editor.Tagging;
using ShaderTools.CodeAnalysis.Shared.TestHooks;
using ShaderTools.CodeAnalysis.Text.Shared.Extensions;
using ShaderTools.Utilities.Threading;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.BraceMatching
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(BraceHighlightTag))]
    [ContentType(ContentTypeNames.ShaderToolsContentType)]
    internal sealed class BraceHighlightingViewTaggerProvider : AsynchronousViewTaggerProvider<BraceHighlightTag>
    {
        [ImportingConstructor]
        public BraceHighlightingViewTaggerProvider(
            IForegroundNotificationService notificationService,
            [ImportMany] IEnumerable<Lazy<IAsynchronousOperationListener, FeatureMetadata>> asyncListeners)
                : base(new AggregateAsynchronousOperationListener(asyncListeners, FeatureAttribute.BraceHighlighting), notificationService)
        {
            
        }

        protected override ITaggerEventSource CreateEventSource(ITextView textView, ITextBuffer subjectBuffer)
        {
            return TaggerEventSources.Compose(
                TaggerEventSources.OnTextChanged(subjectBuffer, TaggerDelay.NearImmediate),
                TaggerEventSources.OnCaretPositionChanged(textView, subjectBuffer, TaggerDelay.NearImmediate));
        }

        protected override Task ProduceTagsAsync(TaggerContext<BraceHighlightTag> context, DocumentSnapshotSpan spanToTag, int? caretPosition)
        {
            var document = spanToTag.Document;
            if (!caretPosition.HasValue || document == null)
            {
                return SpecializedTasks.EmptyTask;
            }

            return ProduceTagsAsync(context, document, spanToTag.SnapshotSpan.Snapshot, caretPosition.Value);
        }

        private async Task ProduceTagsAsync(TaggerContext<BraceHighlightTag> context, Document document, ITextSnapshot snapshot, int position)
        {
            var syntaxTree = await document.GetSyntaxTreeAsync(context.CancellationToken).ConfigureAwait(false);

            var braceMatcher = document.LanguageServices.GetService<IBraceMatcher>();
            if (braceMatcher == null)
                return;

            var mappedPosition = syntaxTree.MapRootFilePosition(position);
            var result = braceMatcher.FindBraces(syntaxTree, mappedPosition, context.CancellationToken);
            if (result == null)
                return;

            context.AddTag(snapshot.GetTagSpan(result.Value.LeftSpan.ToSpan(), BraceHighlightTag.Tag));
            context.AddTag(snapshot.GetTagSpan(result.Value.RightSpan.ToSpan(), BraceHighlightTag.Tag));
        }
    }
}
