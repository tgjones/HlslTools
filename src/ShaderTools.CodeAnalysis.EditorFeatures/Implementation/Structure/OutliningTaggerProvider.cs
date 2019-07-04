using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.CodeAnalysis.Editor.Shared.Tagging;
using ShaderTools.CodeAnalysis.Editor.Tagging;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Shared.TestHooks;
using ShaderTools.CodeAnalysis.Structure;
using ShaderTools.CodeAnalysis.Text.Shared.Extensions;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.Structure
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IOutliningRegionTag))]
    [ContentType(ContentTypeNames.ShaderToolsContentType)]
    internal class OutliningTaggerProvider : AsynchronousTaggerProvider<IOutliningRegionTag>
    {
        [ImportingConstructor]
        public OutliningTaggerProvider(
            IForegroundNotificationService notificationService,
            [ImportMany] IEnumerable<Lazy<IAsynchronousOperationListener, FeatureMetadata>> asyncListeners)
            : base(new AggregateAsynchronousOperationListener(asyncListeners, FeatureAttribute.Outlining), notificationService)
        {
            
        }

        protected override ITaggerEventSource CreateEventSource(ITextView textViewOpt, ITextBuffer subjectBuffer)
        {
            return TaggerEventSources.Compose(
                TaggerEventSources.OnTextChanged(subjectBuffer, TaggerDelay.OnIdle));
        }

        protected override async Task ProduceTagsAsync(TaggerContext<IOutliningRegionTag> context, DocumentSnapshotSpan spanToTag, int? caretPosition)
        {
            var blockStructureProvider = spanToTag.Document.LanguageServices.GetService<IBlockStructureProvider>();
            if (blockStructureProvider == null)
                return;

            var blockSpans = await blockStructureProvider.ProvideBlockStructureAsync(spanToTag.Document, context.CancellationToken).ConfigureAwait(false);

            var snapshot = spanToTag.SnapshotSpan.Snapshot;

            foreach (var blockSpan in blockSpans)
            {
                if (!blockSpan.IsCollapsible)
                    continue;

                var collapsedHintForm = snapshot.GetText(blockSpan.HintSpan.ToSpan());

                context.AddTag(snapshot.GetTagSpan(
                    blockSpan.TextSpan.ToSpan(), 
                    new OutliningRegionTag(false, blockSpan.AutoCollapse, blockSpan.BannerText, collapsedHintForm)));
            }
        }
    }
}
