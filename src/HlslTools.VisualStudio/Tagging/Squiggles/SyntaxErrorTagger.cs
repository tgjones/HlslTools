using System;
using System.Collections.Generic;
using System.Threading;
using HlslTools.Diagnostics;
using HlslTools.VisualStudio.Options;
using HlslTools.VisualStudio.Parsing;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;

namespace HlslTools.VisualStudio.Tagging.Squiggles
{
    internal sealed class SyntaxErrorTagger : ErrorTagger
    {
        public SyntaxErrorTagger(ITextView textView, BackgroundParser backgroundParser,
            IOptionsService optionsService, IServiceProvider serviceProvider,
            ITextDocumentFactoryService textDocumentFactoryService)
            : base(PredefinedErrorTypeNames.SyntaxError, textView, optionsService, serviceProvider, textDocumentFactoryService)
        {
            backgroundParser.SubscribeToThrottledSyntaxTreeAvailable(BackgroundParserSubscriptionDelay.OnIdle,
                async x =>
                {
                    ErrorListHelper.Clear();
                    await InvalidateTags(x.Snapshot, x.CancellationToken);
                });
        }

        protected override IEnumerable<Diagnostic> GetDiagnostics(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            return snapshot.GetSyntaxTree(cancellationToken).GetDiagnostics();
        }
    }
}