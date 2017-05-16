using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using ShaderTools.CodeAnalysis.Hlsl.Formatting;
using ShaderTools.CodeAnalysis.Hlsl.Options;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Editor.VisualStudio.Core.Util;
using ShaderTools.Editor.VisualStudio.Hlsl.Util.Extensions;
using ShaderTools.Utilities.Threading;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Formatting
{
    internal static class FormattingExtensions
    {
        public static void Format(this ITextBuffer buffer, TextSpan span, IHlslOptionsService optionsService)
        {
            SyntaxTree syntaxTree;
            if (!TryGetSyntaxTree(buffer, out syntaxTree))
                return;
            var edits = Formatter.GetEdits(
                syntaxTree,
                span,
                optionsService.GetPrimaryWorkspaceFormattingOptions());
            ApplyEdits(buffer, edits);
        }

        // https://github.com/Microsoft/nodejstools/blob/master/Nodejs/Product/Nodejs/EditFilter.cs#L866
        public static void FormatAfterTyping(this ITextView textView, char ch, IHlslOptionsService optionsService)
        {
            var document = textView.TextBuffer.AsTextContainer().GetOpenDocumentInCurrentContext();

            var documentOptions = document.GetOptionsAsync().WaitAndGetResult(CancellationToken.None);

            if (!ShouldFormatOnCharacter(ch, documentOptions))
                return;

            SyntaxTree syntaxTree;
            if (!TryGetSyntaxTree(textView.TextBuffer, out syntaxTree))
                return;

            var edits = Formatter.GetEditsAfterKeystroke(syntaxTree,
                textView.Caret.Position.BufferPosition.Position, ch,
                optionsService.GetPrimaryWorkspaceFormattingOptions());
            ApplyEdits(textView.TextBuffer, edits);
        }

        private static bool ShouldFormatOnCharacter(char ch, DocumentOptionSet options)
        {
            switch (ch)
            {
                case '}':
                    return options.GetOption(FeatureOnOffOptions.AutoFormattingOnCloseBrace);
                case ';':
                    return options.GetOption(FeatureOnOffOptions.AutoFormattingOnSemicolon);
            }
            return false;
        }

        private static bool TryGetSyntaxTree(ITextBuffer textBuffer, out SyntaxTree syntaxTree)
        {
            try
            {
                var syntaxTreeTask = Task.Run(() => textBuffer.CurrentSnapshot.GetSyntaxTree(CancellationToken.None));

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