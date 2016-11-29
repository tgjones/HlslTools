using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using ShaderTools.Hlsl.Compilation;
using ShaderTools.Hlsl.Diagnostics;
using ShaderTools.VisualStudio.Hlsl.Options;
using ShaderTools.VisualStudio.Hlsl.Parsing;
using ShaderTools.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.VisualStudio.Hlsl.Tagging.Squiggles
{
    internal sealed class SemanticErrorTagger : ErrorTagger
    {
        public SemanticErrorTagger(ITextView textView, BackgroundParser backgroundParser,
            IOptionsService optionsService)
            : base(PredefinedErrorTypeNames.CompilerError, textView, optionsService)
        {
            backgroundParser.SubscribeToThrottledSemanticModelAvailable(BackgroundParserSubscriptionDelay.Medium,
                async x => await InvalidateTags(x.Snapshot, x.CancellationToken));
        }

        protected override IEnumerable<Diagnostic> GetDiagnostics(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            SemanticModel semanticModel;
            if (!snapshot.TryGetSemanticModel(cancellationToken, out semanticModel))
                return Enumerable.Empty<Diagnostic>();
            return semanticModel.GetDiagnostics();
        }
    }
}