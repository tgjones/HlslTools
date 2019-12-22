using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.CodeAnalysis.Editor.Shared.Tagging;
using ShaderTools.CodeAnalysis.Editor.Tagging;
using ShaderTools.CodeAnalysis.Shared.TestHooks;
using ShaderTools.CodeAnalysis.Structure;
using ShaderTools.CodeAnalysis.Text.Shared.Extensions;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.Structure
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IStructureTag))]
    [ContentType(ContentTypeNames.ShaderToolsContentType)]
    internal class StructureTaggerProvider : AsynchronousTaggerProvider<IStructureTag>
    {
        [ImportingConstructor]
        public StructureTaggerProvider(
            IForegroundNotificationService notificationService,
            [ImportMany] IEnumerable<Lazy<IAsynchronousOperationListener, FeatureMetadata>> asyncListeners)
            : base(new AggregateAsynchronousOperationListener(asyncListeners, FeatureAttribute.Structure), notificationService)
        {

        }

        protected override ITaggerEventSource CreateEventSource(ITextView textViewOpt, ITextBuffer subjectBuffer)
        {
            return TaggerEventSources.Compose(
                TaggerEventSources.OnTextChanged(subjectBuffer, TaggerDelay.OnIdle));
        }

        protected override async Task ProduceTagsAsync(TaggerContext<IStructureTag> context, DocumentSnapshotSpan spanToTag, int? caretPosition)
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

                context.AddTag(snapshot.GetTagSpan(
                    blockSpan.TextSpan.ToSpan(),
                    new StructureTag(
                        snapshot,
                        blockSpan.TextSpan.ToSpan(),
                        blockSpan.HintSpan.ToSpan(),
                        isCollapsible: blockSpan.IsCollapsible,
                        isDefaultCollapsed: blockSpan.IsDefaultCollapsed,
                        type: GetStructureTagType(blockSpan.Type))));
            }
        }

        private static string GetStructureTagType(BlockSpanType type)
        {
            switch (type)
            {
                case BlockSpanType.Namespace:
                    return PredefinedStructureTagTypes.Namespace;
                case BlockSpanType.Type:
                    return PredefinedStructureTagTypes.Type;
                case BlockSpanType.Member:
                    return PredefinedStructureTagTypes.Member;
                case BlockSpanType.Conditional:
                    return PredefinedStructureTagTypes.Conditional;
                case BlockSpanType.Loop:
                    return PredefinedStructureTagTypes.Loop;
                case BlockSpanType.PreprocessorRegion:
                    return PredefinedStructureTagTypes.PreprocessorRegion;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
}
