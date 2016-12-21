using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using ShaderTools.Core.Diagnostics;
using ShaderTools.VisualStudio.Core.Parsing;
using ShaderTools.VisualStudio.Core.Tagging.Squiggles;
using ShaderTools.VisualStudio.Hlsl.Options;
using ShaderTools.VisualStudio.Hlsl.Parsing;
using ShaderTools.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.VisualStudio.Hlsl.Tagging.Squiggles
{
    internal sealed class SyntaxErrorTagger : ErrorTagger
    {
        public SyntaxErrorTagger(ITextView textView, ITextBuffer textBuffer, BackgroundParser backgroundParser,
            IHlslOptionsService optionsService)
            : base(PredefinedErrorTypeNames.SyntaxError, textView, textBuffer, optionsService)
        {
            backgroundParser.SubscribeToThrottledSyntaxTreeAvailable(BackgroundParserSubscriptionDelay.Medium,
                async x => await InvalidateTags(x.Snapshot, x.CancellationToken));
        }

        protected override IEnumerable<DiagnosticBase> GetDiagnostics(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            return snapshot.GetSyntaxTree(cancellationToken).GetDiagnostics();
        }
    }
}