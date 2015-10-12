using System.ComponentModel.Composition;
using System.Diagnostics;
using HlslTools.VisualStudio.Options;
using HlslTools.VisualStudio.Tagging.Classification;
using HlslTools.VisualStudio.Text;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.BraceCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace HlslTools.VisualStudio.Editing.BraceCompletion
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
        public HlslClassificationService ClassificationService { get; set; }

        [Import]
        public ISmartIndentationService SmartIndentationService { get; set; }

        [Import]
        public ITextBufferUndoManagerProvider TextBufferUndoManagerProvider { get; set; }

        [Import]
        public IOptionsService OptionsService { get; set; }

        [Import]
        public VisualStudioSourceTextFactory SourceTextFactory { get; set; }

        public bool TryCreateContext(ITextView textView, SnapshotPoint openingPoint, char openingBrace, char closingBrace, out IBraceCompletionContext context)
        {
            // if we are in a comment or string literal we cannot begin a completion session.
            if (IsValidBraceCompletionContext(openingPoint))
            {
                context = new BraceCompletionContext(SmartIndentationService, TextBufferUndoManagerProvider, ClassificationService, OptionsService, SourceTextFactory);
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
                var classifier = openingPoint.Snapshot.TextBuffer.GetSyntaxTagger();
                var snapshotSpan = new SnapshotSpan(openingPoint - 1, 1);
                var classificationSpans = classifier.GetTags(new NormalizedSnapshotSpanCollection(snapshotSpan));

                foreach (var span in classificationSpans)
                {
                    if (!span.Span.OverlapsWith(snapshotSpan))
                        continue;

                    if (span.Tag.ClassificationType.IsOfType("comment"))
                        return false;

                    if (span.Tag.ClassificationType.IsOfType("literal"))
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