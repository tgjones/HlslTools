using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HlslTools.VisualStudio.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace HlslTools.VisualStudio.Tagging.Classification
{
    internal sealed class SyntaxTagger : AsyncTagger<IClassificationTag>, IBackgroundParserSyntaxTreeHandler
    {
        private readonly HlslClassificationService _classificationService;

        public SyntaxTagger(HlslClassificationService classificationService, BackgroundParser backgroundParser)
        {
            _classificationService = classificationService;
            backgroundParser.RegisterSyntaxTreeHandler(BackgroundParserHandlerPriority.Highest, this);
        }

        protected override Tuple<ITextSnapshot, List<ITagSpan<IClassificationTag>>> GetTags(SnapshotSyntaxTree snapshotSyntaxTree, CancellationToken cancellationToken)
        {
            var results = new List<ITagSpan<IClassificationTag>>();
            var worker = new SyntaxTaggerWorker(_classificationService, results, snapshotSyntaxTree.Snapshot, cancellationToken);

            worker.ClassifySyntax(snapshotSyntaxTree.SyntaxTree);
            return Tuple.Create(snapshotSyntaxTree.Snapshot, results);
        }

        async Task IBackgroundParserSyntaxTreeHandler.OnSyntaxTreeAvailable(SnapshotSyntaxTree snapshotSyntaxTree, CancellationToken cancellationToken)
        {
            await InvalidateTags(snapshotSyntaxTree, cancellationToken);
        }
    }
}