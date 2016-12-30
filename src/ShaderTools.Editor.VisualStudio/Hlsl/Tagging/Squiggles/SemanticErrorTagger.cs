using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using ShaderTools.Core.Diagnostics;
using ShaderTools.Hlsl.Compilation;
using ShaderTools.Editor.VisualStudio.Core.Parsing;
using ShaderTools.Editor.VisualStudio.Core.Tagging.Squiggles;
using ShaderTools.Editor.VisualStudio.Hlsl.Options;
using ShaderTools.Editor.VisualStudio.Hlsl.Parsing;
using ShaderTools.Editor.VisualStudio.Hlsl.Util.Extensions;
using System;
using ShaderTools.Core.Syntax;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Squiggles
{
    internal sealed class SemanticErrorTagger : ErrorTagger
    {
        public SemanticErrorTagger(ITextView textView, ITextBuffer textBuffer, BackgroundParser backgroundParser,
            IHlslOptionsService optionsService)
            : base(PredefinedErrorTypeNames.CompilerError, textView, textBuffer, optionsService)
        {
            backgroundParser.SubscribeToThrottledSemanticModelAvailable(BackgroundParserSubscriptionDelay.Medium,
                async x => await InvalidateTags(x.Snapshot, x.CancellationToken));
        }

        protected override Tuple<SyntaxTreeBase, IEnumerable<Diagnostic>> GetDiagnostics(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            SemanticModel semanticModel;
            if (!snapshot.TryGetSemanticModel(cancellationToken, out semanticModel))
                return Tuple.Create<SyntaxTreeBase, IEnumerable<Diagnostic>>(null, Enumerable.Empty<Diagnostic>());
            return Tuple.Create((SyntaxTreeBase) semanticModel.SyntaxTree, semanticModel.GetDiagnostics());
        }
    }
}