using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using ShaderTools.VisualStudio.Hlsl.Parsing;
using ShaderTools.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.VisualStudio.Hlsl.Tagging.Classification
{
    internal sealed class SyntaxTagger : AsyncTagger<IClassificationTag>
    {
        private readonly HlslClassificationService _classificationService;

        public SyntaxTagger(HlslClassificationService classificationService, BackgroundParser backgroundParser)
        {
            _classificationService = classificationService;

            backgroundParser.SubscribeToThrottledSyntaxTreeAvailable(BackgroundParserSubscriptionDelay.NearImmediate,
                async x => await InvalidateTags(x.Snapshot, x.CancellationToken));
        }

        protected override Tuple<ITextSnapshot, List<ITagSpan<IClassificationTag>>> GetTags(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            var results = new List<ITagSpan<IClassificationTag>>();
            var worker = new SyntaxTaggerWorker(_classificationService, results, snapshot, cancellationToken);

            worker.ClassifySyntax(snapshot.GetSyntaxTree(cancellationToken));
            return Tuple.Create(snapshot, results);
        }
    }
}