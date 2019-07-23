using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Options;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Editor.Shared.Tagging;
using ShaderTools.CodeAnalysis.Editor.Tagging;
using ShaderTools.CodeAnalysis.Shared.TestHooks;
using ShaderTools.CodeAnalysis.Text.Shared.Extensions;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.Diagnostics
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IErrorTag))]
    [ContentType(ContentTypeNames.ShaderToolsContentType)]
    internal sealed class DiagnosticsSquiggleTaggerProvider : AsynchronousTaggerProvider<IErrorTag>
    {
        private readonly IDiagnosticService _diagnosticService;
        private readonly ImmutableArray<PerLanguageOption<bool>> _options;

        [ImportingConstructor]
        public DiagnosticsSquiggleTaggerProvider(
            IForegroundNotificationService notificationService,
            [ImportMany] IEnumerable<Lazy<IAsynchronousOperationListener, FeatureMetadata>> asyncListeners)
            : base(new AggregateAsynchronousOperationListener(asyncListeners, FeatureAttribute.DiagnosticService), notificationService)
        {            
            _diagnosticService = PrimaryWorkspace.Workspace.Services.GetRequiredService<IDiagnosticService>();
            _options = new[] { DiagnosticsOptions.EnableErrorReporting, DiagnosticsOptions.EnableSquiggles }.ToImmutableArray();
        }

        protected override IEnumerable<PerLanguageOption<bool>> PerLanguageOptions => _options;

        protected override ITaggerEventSource CreateEventSource(ITextView textViewOpt, ITextBuffer subjectBuffer)
        {
            return TaggerEventSources.OnDiagnosticsChanged(subjectBuffer, _diagnosticService, TaggerDelay.OnIdle);
        }

        protected override async Task ProduceTagsAsync(TaggerContext<IErrorTag> context, DocumentSnapshotSpan spanToTag, int? caretPosition)
        {
            var document = spanToTag.Document;
            var snapshot = spanToTag.SnapshotSpan.Snapshot;

            var diagnostics = await _diagnosticService.GetDiagnosticsAsync(document.Id, context.CancellationToken);

            AddDiagnostics(context, diagnostics, snapshot);
        }

        private static void AddDiagnostics(TaggerContext<IErrorTag> context, IEnumerable<MappedDiagnostic> mappedDiagnostics, ITextSnapshot snapshot)
        {
            foreach (var mappedDiagnostic in mappedDiagnostics)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (!mappedDiagnostic.FileSpan.IsInRootFile)
                    continue;

                var errorType = mappedDiagnostic.Diagnostic.Severity == DiagnosticSeverity.Warning
                    ? PredefinedErrorTypeNames.Warning
                    : (mappedDiagnostic.Source == DiagnosticSource.SyntaxParsing ? PredefinedErrorTypeNames.SyntaxError : PredefinedErrorTypeNames.CompilerError);

                var tag = new ErrorTag(errorType, mappedDiagnostic.Diagnostic.Message);

                context.AddTag(snapshot.GetTagSpan(mappedDiagnostic.FileSpan.Span.ToSpan(), tag));
            }
        }
    }
}
