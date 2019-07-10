using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.BraceCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.CodeAnalysis.Classification;
using ShaderTools.CodeAnalysis.Editor.Shared.Utilities;
using ShaderTools.CodeAnalysis.Shared.Extensions;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.BraceCompletion
{
    [Export(typeof(IBraceCompletionContextProvider))]
    [BracePair(BraceKind.CurlyBrackets.Open, BraceKind.CurlyBrackets.Close)]
    [BracePair(BraceKind.SquareBrackets.Open, BraceKind.SquareBrackets.Close)]
    [BracePair(BraceKind.Parentheses.Open, BraceKind.Parentheses.Close)]
    [BracePair(BraceKind.SingleQuotes.Open, BraceKind.SingleQuotes.Close)]
    [BracePair(BraceKind.DoubleQuotes.Open, BraceKind.DoubleQuotes.Close)]
    [ContentType(ContentTypeNames.ShaderToolsContentType)]
    internal sealed class BraceCompletionContextProvider : ForegroundThreadAffinitizedObject, IBraceCompletionContextProvider
    {
        private readonly ISmartIndentationService _smartIndentationService;
        private readonly ITextBufferUndoManagerProvider _textBufferUndoManagerProvider;

        [ImportingConstructor]
        public BraceCompletionContextProvider(ISmartIndentationService smartIndentationService, ITextBufferUndoManagerProvider textBufferUndoManagerProvider)
        {
            _smartIndentationService = smartIndentationService;
            _textBufferUndoManagerProvider = textBufferUndoManagerProvider;
        }

        public bool TryCreateContext(ITextView textView, SnapshotPoint openingPoint, char openingBrace, char closingBrace, out IBraceCompletionContext context)
        {
            this.AssertIsForeground();

            if (IsValidBraceCompletionContext(openingPoint))
            {
                var textSnapshot = openingPoint.Snapshot;
                var document = textSnapshot.GetOpenDocumentInCurrentContextWithChanges();
                if (document != null)
                {
                    var editorSessionFactory = document.GetLanguageService<IEditorBraceCompletionSessionFactory>();
                    if (editorSessionFactory != null)
                    {
                        // Brace completion is (currently) not cancellable.
                        var cancellationToken = CancellationToken.None;

                        var editorSession = editorSessionFactory.TryCreateSession(document, openingPoint, openingBrace, cancellationToken);
                        if (editorSession != null)
                        {
                            var undoManager = _textBufferUndoManagerProvider.GetTextBufferUndoManager(textView.TextBuffer);
                            context = new BraceCompletionContext(_smartIndentationService, undoManager, editorSession);
                            return true;
                        }
                    }
                }
            }

            context = null;
            return false;
        }

        private static bool IsValidBraceCompletionContext(SnapshotPoint openingPoint)
        {
            Debug.Assert(openingPoint.Position >= 0, "SnapshotPoint.Position should always be zero or positive.");

            if (openingPoint.Position > 0)
            {
                var document = openingPoint.Snapshot.AsText().GetOpenDocumentInCurrentContextWithChanges();
                var syntaxTree = document.GetSyntaxTreeAsync(CancellationToken.None).Result; // TODO: Don't do this.

                var classificationService = document.LanguageServices.GetRequiredService<IClassificationService>();

                var classificationSpans = new List<ClassifiedSpan>();

                var textSpan = new TextSpan(openingPoint.Position - 1, 1);
                classificationService.AddSyntacticClassifications(syntaxTree, textSpan, classificationSpans, CancellationToken.None);

                foreach (var span in classificationSpans)
                {
                    if (!span.TextSpan.OverlapsWith(textSpan))
                        continue;

                    if (span.ClassificationType == ClassificationTypeNames.Comment)
                        return false;

                    if (span.ClassificationType == ClassificationTypeNames.NumericLiteral || span.ClassificationType == ClassificationTypeNames.StringLiteral)
                        return false;
                }
            }

            // If we haven't stopped this, go ahead and start the completion session.
            // Either we were at position 0 (a safe place to be placing a brace completion)
            // or we were in a classification that is safe for brace completion.
            return true;
        }
    }
}