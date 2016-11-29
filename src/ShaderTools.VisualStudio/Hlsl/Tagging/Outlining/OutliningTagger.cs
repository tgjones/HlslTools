using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.VisualStudio.Hlsl.Options;
using ShaderTools.VisualStudio.Hlsl.Parsing;
using ShaderTools.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.VisualStudio.Hlsl.Tagging.Outlining
{
    internal sealed class OutliningTagger : AsyncTagger<IOutliningRegionTag>
    {
        private readonly ITextBuffer _textBuffer;
        private bool _enabled;

        public OutliningTagger(ITextBuffer textBuffer, BackgroundParser backgroundParser, IOptionsService optionsService)
        {
            _textBuffer = textBuffer;

            backgroundParser.SubscribeToThrottledSyntaxTreeAvailable(BackgroundParserSubscriptionDelay.OnIdle,
                async x => await InvalidateTags(x.Snapshot, x.CancellationToken));

            _enabled = optionsService.AdvancedOptions.EnterOutliningModeWhenFilesOpen;
        }

        protected override Tuple<ITextSnapshot, List<ITagSpan<IOutliningRegionTag>>> GetTags(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            if (!_enabled)
                return Tuple.Create(snapshot, new List<ITagSpan<IOutliningRegionTag>>());

            var outliningRegions = new List<ITagSpan<IOutliningRegionTag>>();
            var outliningVisitor = new OutliningVisitor(snapshot, outliningRegions, cancellationToken);

            outliningVisitor.VisitCompilationUnit((CompilationUnitSyntax) snapshot.GetSyntaxTree(cancellationToken).Root);

            return Tuple.Create(snapshot, outliningRegions);
        }

        public void UpdateEnabled(bool enabled)
        {
            _enabled = enabled;

#pragma warning disable CS4014
            InvalidateTags(_textBuffer.CurrentSnapshot, CancellationToken.None);
#pragma warning restore CS4014
        }
    }
}