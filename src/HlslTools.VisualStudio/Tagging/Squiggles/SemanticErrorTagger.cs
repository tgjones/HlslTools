using System;
using System.Collections.Generic;
using HlslTools.Diagnostics;
using HlslTools.VisualStudio.Options;
using HlslTools.VisualStudio.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;

namespace HlslTools.VisualStudio.Tagging.Squiggles
{
    internal sealed class SemanticErrorTagger : ErrorTagger
    {
        public SemanticErrorTagger(ITextView textView, BackgroundParser backgroundParser,
            IOptionsService optionsService, IServiceProvider serviceProvider,
            ITextDocumentFactoryService textDocumentFactoryService)
            : base(PredefinedErrorTypeNames.SyntaxError, textView, backgroundParser, optionsService, serviceProvider, textDocumentFactoryService)
        {

        }

        protected override IEnumerable<Diagnostic> GetDiagnostics(SnapshotSyntaxTree snapshotSyntaxTree)
        {
            return snapshotSyntaxTree.SemanticModel.GetDiagnostics();
        }
    }
}