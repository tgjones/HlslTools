using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HlslTools.Diagnostics;
using HlslTools.VisualStudio.Options;
using HlslTools.VisualStudio.Parsing;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;

namespace HlslTools.VisualStudio.Tagging.Squiggles
{
    internal sealed class SyntaxErrorTagger : ErrorTagger, IBackgroundParserSyntaxTreeHandler
    {
        public SyntaxErrorTagger(ITextView textView, BackgroundParser backgroundParser,
            IOptionsService optionsService, IServiceProvider serviceProvider,
            ITextDocumentFactoryService textDocumentFactoryService)
            : base(PredefinedErrorTypeNames.SyntaxError, textView, optionsService, serviceProvider, textDocumentFactoryService)
        {
            backgroundParser.RegisterSyntaxTreeHandler(BackgroundParserHandlerPriority.Low, this);
        }

        async Task IBackgroundParserSyntaxTreeHandler.OnSyntaxTreeAvailable(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            ErrorListHelper.Clear();
            await InvalidateTags(snapshot, cancellationToken);
        }

        protected override IEnumerable<Diagnostic> GetDiagnostics(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            return snapshot.GetSyntaxTree(cancellationToken).GetDiagnostics();
        }
    }
}