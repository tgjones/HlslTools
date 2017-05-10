using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Editor.Shared.Tagging;
using ShaderTools.CodeAnalysis.Editor.Tagging;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Shared.TestHooks;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text.Shared.Extensions;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.Diagnostics
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IErrorTag))]
    [ContentType(ContentTypeNames.ShaderToolsContentType)]
    internal sealed class DiagnosticsSquiggleTaggerProvider : AsynchronousTaggerProvider<IErrorTag>
    {
        private readonly ImmutableArray<PerLanguageOption<bool>> _options;

        [ImportingConstructor]
        public DiagnosticsSquiggleTaggerProvider(
            IForegroundNotificationService notificationService,
            [ImportMany] IEnumerable<Lazy<IAsynchronousOperationListener, FeatureMetadata>> asyncListeners)
            : base(new AggregateAsynchronousOperationListener(asyncListeners, FeatureAttribute.DiagnosticService), notificationService)
        {
            _options = new[] { DiagnosticsOptions.EnableErrorReporting, DiagnosticsOptions.EnableSquiggles }.ToImmutableArray();
        }

        protected override IEnumerable<PerLanguageOption<bool>> PerLanguageOptions => _options;

        protected override ITaggerEventSource CreateEventSource(ITextView textViewOpt, ITextBuffer subjectBuffer)
        {
            return TaggerEventSources.OnTextChanged(subjectBuffer, TaggerDelay.OnIdle);
        }

        protected override async Task ProduceTagsAsync(TaggerContext<IErrorTag> context, DocumentSnapshotSpan spanToTag, int? caretPosition)
        {
            var document = spanToTag.Document;
            var snapshot = spanToTag.SnapshotSpan.Snapshot;

            // TODO: Remove this.
            var optionsService = document.LanguageServices.GetRequiredService<IOptionsService>();
            if (!optionsService.EnableErrorReporting || !optionsService.EnableSquiggles)
                return;

            var syntaxTree = await document.GetSyntaxTreeAsync(context.CancellationToken).ConfigureAwait(false);
            if (syntaxTree == null)
                return;

            AddDiagnostics(context, syntaxTree.GetDiagnostics(), snapshot, syntaxTree, true);

            var semanticModel = await document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            if (semanticModel != null)
                AddDiagnostics(context, semanticModel.GetDiagnostics(), snapshot, syntaxTree, true);
        }

        private void AddDiagnostics(TaggerContext<IErrorTag> context, IEnumerable<Diagnostic> diagnostics, ITextSnapshot snapshot, SyntaxTreeBase syntaxTree, bool isSyntax)
        {
            foreach (var diagnostic in diagnostics)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                var diagnosticTextSpan = syntaxTree.GetSourceFileSpan(diagnostic.SourceRange);
                if (!diagnosticTextSpan.IsInRootFile)
                    continue;

                var errorType = diagnostic.Severity == DiagnosticSeverity.Warning
                    ? PredefinedErrorTypeNames.Warning
                    : (isSyntax ? PredefinedErrorTypeNames.SyntaxError : PredefinedErrorTypeNames.CompilerError);

                var tag = new ErrorTag(errorType, diagnostic.Message);

                context.AddTag(snapshot.GetTagSpan(diagnosticTextSpan.Span.ToSpan(), tag));
            }
        }
    }
}
