using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using HlslTools.Compilation;
using HlslTools.Syntax;
using HlslTools.VisualStudio.Glyphs;

namespace HlslTools.VisualStudio.IntelliSense.Completion.CompletionProviders
{
    //[Export(typeof(ICompletionProvider))]
    internal sealed class KeywordCompletionProvider : ICompletionProvider
    {
        public IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, SourceLocation position)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var root = syntaxTree.Root;

            if (root.InComment(position) || root.InLiteral(position) || IsInPropertyAccess(root, position))
                return Enumerable.Empty<CompletionItem>();

            // We don't want to show a keyword completion in a macro.
            if (semanticModel.SyntaxTree.DefinitelyInMacro(position))
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

            var propertyAccess = token.Parent.AncestorsAndSelf().OfType<FieldAccessExpressionSyntax>().FirstOrDefault();
            return propertyAccess != null && (propertyAccess.DotToken == token || propertyAccess.Name == token);
        }

        private static IEnumerable<SyntaxKind> GetAvailableKeywords(SyntaxTree syntaxTree, SourceLocation position)
        {
            if (IsInSemantic(syntaxTree, position))
            {
                yield return SyntaxKind.PackoffsetKeyword;
                yield return SyntaxKind.RegisterKeyword;
            }

            yield return SyntaxKind.TrueKeyword;
            yield return SyntaxKind.FalseKeyword;
        }

        private static bool IsInSemantic(SyntaxTree syntaxTree, SourceLocation position)
        {
            var token = syntaxTree.Root.FindTokenOnLeft(position);
            return token.Parent
                .AncestorsAndSelf()
                .OfType<SemanticSyntax>()
                .Any();
        }
    }
}