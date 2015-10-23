using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using HlslTools.Compilation;
using HlslTools.Syntax;
using HlslTools.VisualStudio.Glyphs;

namespace HlslTools.VisualStudio.IntelliSense.Completion.CompletionProviders
{
    [Export(typeof(ICompletionProvider))]
    internal sealed class KeywordCompletionProvider : ICompletionProvider
    {
        public IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, SourceLocation position)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var root = syntaxTree.Root;

            if (root.InComment(position)
                || root.InLiteral(position)
                || IsInPropertyAccess(root, position))
                return Enumerable.Empty<CompletionItem>();

            return GetAvailableKeywords(syntaxTree, position)
                .Select(k => k.GetText())
                .Select(t => new CompletionItem(t, t, t + " Keyword", Glyph.Keyword));
        }
        
        private static bool IsInPropertyAccess(SyntaxNode root, SourceLocation position)
        {
            var token = root.FindTokenOnLeft(position);
            if (token == null || !token.SourceRange.ContainsOrTouches(position))
                return false;

            var propertyAccess = token.Parent.AncestorsAndSelf().OfType<MemberAccessExpressionSyntax>().FirstOrDefault();
            return propertyAccess != null && (propertyAccess.DotToken == token || propertyAccess.Name.Name == token);
        }

        private static IEnumerable<SyntaxKind> GetAvailableKeywords(SyntaxTree syntaxTree, SourceLocation position)
        {
            yield return SyntaxKind.TrueKeyword;
            yield return SyntaxKind.FalseKeyword;
        }
    }
}