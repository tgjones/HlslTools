using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;

namespace ShaderTools.CodeAnalysis.Hlsl.Formatting
{
    public static class Formatter
    {
        public static IList<TextChange> GetEdits(SyntaxTree syntaxTree, SyntaxNode syntaxNode, TextSpan textSpan, FormattingOptions options)
        {
            // Format.
            var formattingVisitor = new FormattingVisitor(syntaxTree, textSpan, options);
            formattingVisitor.Visit(syntaxNode);

            return formattingVisitor.Edits.Values;
        }

        public static IList<TextChange> GetEditsAfterKeystroke(SyntaxTree syntaxTree, int position, char character, FormattingOptions options)
        {
            var location = syntaxTree.MapRootFilePosition(position);

            // Find span of block / statement terminated by the character just typed.
            var token = ((SyntaxNode) syntaxTree.Root).FindTokenOnLeft(location);
            if (token.Text != character.ToString())
                return new List<TextChange>();

            // Get span of node containing this token.
            var span = token.Parent.GetTextSpanRoot();
            if (span == null)
                return new List<TextChange>();

            return GetEdits(syntaxTree, (SyntaxNode) syntaxTree.Root, span.Value.Span, options);
        }

        public static string ApplyEdits(string code, IList<TextChange> edits)
        {
            var sortedEdits = edits.OrderBy(x => x.Span.Start);
            var newCode = new StringBuilder(code);
            int delta = 0;
            foreach (var edit in sortedEdits)
            {
                newCode.Remove(edit.Span.Start + delta, edit.Span.Length);
                newCode.Insert(edit.Span.Start + delta, edit.NewText);
                delta -= edit.Span.Length;
                delta += edit.NewText.Length;
            }
            return newCode.ToString();
        }
    }
}