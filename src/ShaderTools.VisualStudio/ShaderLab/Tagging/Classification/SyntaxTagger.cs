using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using ShaderTools.VisualStudio.Core.Parsing;
using ShaderTools.VisualStudio.Core.Tagging;
using ShaderTools.VisualStudio.ShaderLab.Parsing;
using ShaderTools.VisualStudio.ShaderLab.Util.Extensions;

namespace ShaderTools.VisualStudio.ShaderLab.Tagging.Classification
{
    internal sealed class SyntaxTagger : AsyncTagger<IClassificationTag>
    {
        private readonly ShaderLabClassificationService _classificationService;

        public SyntaxTagger(ShaderLabClassificationService classificationService, BackgroundParser backgroundParser)
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