using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HlslTools.Compilation;
using HlslTools.Diagnostics;
using HlslTools.VisualStudio.Options;
using HlslTools.VisualStudio.Parsing;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;

namespace HlslTools.VisualStudio.Tagging.Squiggles
{
    internal sealed class SemanticErrorTagger : ErrorTagger, IBackgroundParserSemanticModelHandler
    {
        public SemanticErrorTagger(ITextView textView, BackgroundParser backgroundParser,
            IOptionsService optionsService, IServiceProvider serviceProvider,
            ITextDocumentFactoryService textDocumentFactoryService)
            : base(PredefinedErrorTypeNames.CompilerError, textView, optionsService, serviceProvider, textDocumentFactoryService)
        {
            backgroundParser.RegisterSemanticModelHandler(BackgroundParserHandlerPriority.Low, this);
        }

        async Task IBackgroundParserSemanticModelHandler.OnSemanticModelAvailable(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            ErrorListHelper.Clear();
            await InvalidateTags(snapshot, cancellationToken);
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