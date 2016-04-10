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
            IOptionsService optionsService)
            : base(PredefinedErrorTypeNames.SyntaxError, textView, optionsService)
        {
            backgroundParser.SubscribeToThrottledSyntaxTreeAvailable(BackgroundParserSubscriptionDelay.Medium,
                async x => await InvalidateTags(x.Snapshot, x.CancellationToken));
        }

        protected override IEnumerable<Diagnostic> GetDiagnostics(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            return snapshot.GetSyntaxTree(cancellationToken).GetDiagnostics();
        }
    }
}