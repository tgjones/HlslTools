using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HlslTools.Formatting;
using HlslTools.Syntax;
using HlslTools.Text;
using HlslTools.VisualStudio.Options;
using HlslTools.VisualStudio.Text;
using HlslTools.VisualStudio.Util;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace HlslTools.VisualStudio.Formatting
{
    internal static class FormattingExtensions
    {
        public static void Format(this ITextBuffer buffer, TextSpan span, IOptionsService optionsService, VisualStudioSourceTextFactory sourceTextFactory)
        {
            SyntaxTree syntaxTree;
            if (!TryGetSyntaxTree(buffer, sourceTextFactory, out syntaxTree))
                return;
            var edits = Formatter.GetEdits(syntaxTree,
                span,
                optionsService.FormattingOptions);
            ApplyEdits(buffer, edits);
        }

        // https://github.com/Microsoft/nodejstools/blob/master/Nodejs/Product/Nodejs/EditFilter.cs#L866
        public static void FormatAfterTyping(this ITextView textView, char ch, IOptionsService optionsService, VisualStudioSourceTextFactory sourceTextFactory)
        {
            if (!ShouldFormatOnCharacter(ch, optionsService))
                return;

            SyntaxTree syntaxTree;
            if (!TryGetSyntaxTree(textView.TextBuffer, sourceTextFactory, out syntaxTree))
                return;

            var edits = Formatter.GetEditsAfterKeystroke(syntaxTree,
                textView.Caret.Position.BufferPosition.Position, ch,
                optionsService.FormattingOptions);
            ApplyEdits(textView.TextBuffer, edits);
        }

        private static bool ShouldFormatOnCharacter(char ch, IOptionsService optionsService)
        {
            switch (ch)
            {
                case '}':
                    return optionsService.GeneralOptions.AutomaticallyFormatBlockOnCloseBrace;
                case ';':
                    return optionsService.GeneralOptions.AutomaticallyFormatStatementOnSemicolon;
            }
            return false;
        }

        private static bool TryGetSyntaxTree(ITextBuffer textBuffer, VisualStudioSourceTextFactory sourceTextFactory, out SyntaxTree syntaxTree)
        {
            try
            {
                var syntaxTreeTask = Task.Run(() => textBuffer.CurrentSnapshot.GetSyntaxTree(sourceTextFactory, CancellationToken.None));

                if (!syntaxTreeTask.Wait(TimeSpan.FromSeconds(5)))
                {
                    Logger.Log("Parsing timeout");
                    syntaxTree = null;
                    return false;
                }

                syntaxTree = syntaxTreeTask.Result;
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log("Parsing error: " + ex);
                syntaxTree = null;
                return false;
            }
        }

        private static void ApplyEdits(ITextBuffer textBuffer, IList<Edit> edits)
        {
            using (var vsEdit = textBuffer.CreateEdit())
            {
                foreach (var edit in edits)
                    vsEdit.Replace(edit.Start, edit.Length, edit.Text);
                vsEdit.Apply();
            }
        }
    }
}