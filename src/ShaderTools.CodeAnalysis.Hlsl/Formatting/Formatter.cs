using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Formatting
{
    public static class Formatter
    {
        public static IList<Edit> GetEdits(SyntaxTree syntaxTree, TextSpan textSpan, FormattingOptions options)
        {
            // Format.
            var formattingVisitor = new FormattingVisitor(syntaxTree, textSpan, options);
            formattingVisitor.Visit((SyntaxNode) syntaxTree.Root);

            return formattingVisitor.Edits.Values;
        }

        public static IList<Edit> GetEditsAfterKeystroke(SyntaxTree syntaxTree, int position, char character, FormattingOptions options)
        {
            var location = syntaxTree.MapRootFilePosition(position);

            // Find span of block / statement terminated by the character just typed.
            var token = ((SyntaxNode) syntaxTree.Root).FindTokenOnLeft(location);
            if (token.Text != character.ToString())
                return new List<Edit>();

            // Get span of node containing this token.
            var span = token.Parent.GetTextSpanRoot();
            if (span == null)
                return new List<Edit>();

            return GetEdits(syntaxTree, span.Value.Span, options);
        }

        public static string ApplyEdits(string code, IList<Edit> edits)
        {
            var sortedEdits = edits.OrderBy(x => x.Start);
            var newCode = new StringBuilder(code);
            int delta = 0;
            foreach (var edit in sortedEdits)
            {
                newCode.Remove(edit.Start + delta, edit.Length);
                newCode.Insert(edit.Start + delta, edit.Text);
                delta -= edit.Length;
                delta += edit.Text.Length;
            }
            return newCode.ToString();
        }
    }
}