using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.BraceCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.CodeAnalysis.Classification;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Editor.VisualStudio.Hlsl.Options;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Editing.BraceCompletion
{
    [Export(typeof(IBraceCompletionContextProvider))]
    [BracePair(BraceKind.CurlyBrackets.Open, BraceKind.CurlyBrackets.Close)]
    [BracePair(BraceKind.SquareBrackets.Open, BraceKind.SquareBrackets.Close)]
    [BracePair(BraceKind.Parentheses.Open, BraceKind.Parentheses.Close)]
    [BracePair(BraceKind.SingleQuotes.Open, BraceKind.SingleQuotes.Close)]
    [BracePair(BraceKind.DoubleQuotes.Open, BraceKind.DoubleQuotes.Close)]
    [ContentType(HlslConstants.ContentTypeName)]
    internal sealed class BraceCompletionContextProvider : IBraceCompletionContextProvider
    {
        [Import]
        public ISmartIndentationService SmartIndentationService { get; set; }

        [Import]
        public ITextBufferUndoManagerProvider TextBufferUndoManagerProvider { get; set; }

        [Import]
        public IHlslOptionsService OptionsService { get; set; }

        public bool TryCreateContext(ITextView textView, SnapshotPoint openingPoint, char openingBrace, char closingBrace, out IBraceCompletionContext context)
        {
            // if we are in a comment or string literal we cannot begin a completion session.
            if (IsValidBraceCompletionContext(openingPoint))
            {
                context = new BraceCompletionContext(SmartIndentationService, TextBufferUndoManagerProvider, OptionsService);
                return true;
            }
            else
            {
                context = null;
                return false;
            }
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