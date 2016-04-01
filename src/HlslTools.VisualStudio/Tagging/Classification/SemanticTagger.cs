using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HlslTools.Syntax;
using HlslTools.VisualStudio.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace HlslTools.VisualStudio.Tagging.Classification
{
    internal sealed class SemanticTagger : AsyncTagger<IClassificationTag>, IBackgroundParserSyntaxTreeHandler
    {
        private readonly HlslClassificationService _classificationService;

        public SemanticTagger(HlslClassificationService classificationService, BackgroundParser backgroundParser)
        {
            _classificationService = classificationService;
            backgroundParser.RegisterSyntaxTreeHandler(BackgroundParserHandlerPriority.High, this);
        }

        protected override Tuple<ITextSnapshot, List<ITagSpan<IClassificationTag>>> GetTags(SnapshotSyntaxTree snapshotSyntaxTree, CancellationToken cancellationToken)
        {
            var semanticTags = new List<ITagSpan<IClassificationTag>>();

            if (snapshotSyntaxTree.SemanticModel != null)
            {
                var semanticTaggerVisitor = new SemanticTaggerVisitor(snapshotSyntaxTree.SemanticModel, _classificationService, snapshotSyntaxTree.Snapshot, semanticTags, cancellationToken);
                semanticTaggerVisitor.VisitCompilationUnit((CompilationUnitSyntax) snapshotSyntaxTree.SyntaxTree.Root);
            }

            return Tuple.Create(snapshotSyntaxTree.Snapshot, semanticTags);
        }

        async Task IBackgroundParserSyntaxTreeHandler.OnSyntaxTreeAvailable(SnapshotSyntaxTree snapshotSyntaxTree, CancellationToken cancellationToken)
        {
            await InvalidateTags(snapshotSyntaxTree, cancellationToken);
        }
    }
}