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
using ShaderTools.CodeAnalysis.Shared.TestHooks;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.Classification
{
    [Export(typeof(ITaggerProvider))]
    [ContentType(ContentTypeNames.ShaderToolsContentType)]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [TagType(typeof(IClassificationTag))]
    internal sealed class SyntacticClassificationTaggerProvider : AsynchronousTaggerProvider<IClassificationTag>
    {
        private readonly ClassificationTypeMap _typeMap;

        [ImportingConstructor]
        public SyntacticClassificationTaggerProvider(
            ClassificationTypeMap typeMap,
            IForegroundNotificationService notificationService,
            [ImportMany] IEnumerable<Lazy<IAsynchronousOperationListener, FeatureMetadata>> asyncListeners) 
            : base(new AggregateAsynchronousOperationListener(asyncListeners, FeatureAttribute.KeywordHighlighting), notificationService)
        {
            _typeMap = typeMap;
        }

        protected override ITaggerEventSource CreateEventSource(ITextView textViewOpt, ITextBuffer subjectBuffer)
        {
            this.AssertIsForeground();

            return TaggerEventSources.OnTextChanged(subjectBuffer, delay: TaggerDelay.NearImmediate);
        }

        protected override async Task ProduceTagsAsync(TaggerContext<IClassificationTag> context, DocumentSnapshotSpan spanToTag, int? caretPosition)
        {
            var document = spanToTag.Document;
            var classificationService = document?.LanguageServices.GetService<IClassificationService>();

            if (classificationService == null)
            {
                return;
            }

            var syntaxTree = await document.GetSyntaxTreeAsync(context.CancellationToken).ConfigureAwait(false);

            var classifiedSpans = ClassificationUtilities.GetOrCreateClassifiedSpanList();

            classificationService.AddSyntacticClassifications(
                syntaxTree, 
                spanToTag.SnapshotSpan.Span.ToTextSpan(),
                classifiedSpans, 
                context.CancellationToken);

            ClassificationUtilities.Convert(_typeMap, spanToTag.SnapshotSpan.Snapshot, classifiedSpans, context.AddTag);
            ClassificationUtilities.ReturnClassifiedSpanList(classifiedSpans);
        }
    }
}
