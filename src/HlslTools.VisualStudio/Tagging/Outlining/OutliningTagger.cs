using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HlslTools.Syntax;
using HlslTools.VisualStudio.Options;
using HlslTools.VisualStudio.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace HlslTools.VisualStudio.Tagging.Outlining
{
    internal sealed class OutliningTagger : AsyncTagger<IOutliningRegionTag>, IBackgroundParserSyntaxTreeHandler
    {
        private SnapshotSyntaxTree _latestSnapshotSyntaxTree;
        private bool _enabled;

        public OutliningTagger(BackgroundParser backgroundParser, IOptionsService optionsService)
        {
            backgroundParser.RegisterSyntaxTreeHandler(BackgroundParserHandlerPriority.Medium, this);

            _enabled = optionsService.AdvancedOptions.EnterOutliningModeWhenFilesOpen;
        }

        protected override Tuple<ITextSnapshot, List<ITagSpan<IOutliningRegionTag>>> GetTags(SnapshotSyntaxTree snapshotSyntaxTree, CancellationToken cancellationToken)
        {
            if (!_enabled)
                return Tuple.Create(snapshotSyntaxTree.Snapshot, new List<ITagSpan<IOutliningRegionTag>>());

            var outliningRegions = new List<ITagSpan<IOutliningRegionTag>>();
            var outliningVisitor = new OutliningVisitor(snapshotSyntaxTree.Snapshot, outliningRegions, cancellationToken);

            outliningVisitor.VisitCompilationUnit((CompilationUnitSyntax) snapshotSyntaxTree.SyntaxTree.Root);

            return Tuple.Create(snapshotSyntaxTree.Snapshot, outliningRegions);
        }

        async Task IBackgroundParserSyntaxTreeHandler.OnSyntaxTreeAvailable(SnapshotSyntaxTree snapshotSyntaxTree, CancellationToken cancellationToken)
        {
            await InvalidateTags(snapshotSyntaxTree, cancellationToken);
        }

        public override Task InvalidateTags(SnapshotSyntaxTree snapshotSyntaxTree, CancellationToken cancellationToken)
        {
            _latestSnapshotSyntaxTree = snapshotSyntaxTree;
            return base.InvalidateTags(snapshotSyntaxTree, cancellationToken);
        }

        public void UpdateEnabled(bool enabled)
        {
            _enabled = enabled;

            if (_latestSnapshotSyntaxTree == null)
                return;

#pragma warning disable CS4014
            InvalidateTags(_latestSnapshotSyntaxTree, CancellationToken.None);
#pragma warning restore CS4014
        }
    }
}