using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using HlslTools.Compilation;
using HlslTools.Syntax;
using HlslTools.VisualStudio.Glyphs;
using HlslTools.VisualStudio.Util.ContextQuery;

namespace HlslTools.VisualStudio.IntelliSense.Completion.CompletionProviders
{
    [Export(typeof(ICompletionProvider))]
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
            var tokenOnLeftOfPosition = syntaxTree.Root.FindTokenOnLeft(position);

            if (IsInSemantic(syntaxTree, tokenOnLeftOfPosition))
            {
                yield return SyntaxKind.PackoffsetKeyword;
                yield return SyntaxKind.RegisterKeyword;
            }

            if (IsStatementContext(syntaxTree, position, tokenOnLeftOfPosition))
            {
                yield return SyntaxKind.ReturnKeyword;
            }

            yield return SyntaxKind.TrueKeyword;
            yield return SyntaxKind.FalseKeyword;
        }

        private static bool IsInSemantic(SyntaxTree syntaxTree, SyntaxToken tokenOnLeftOfPosition)
        {
            return tokenOnLeftOfPosition.GetAncestor<SemanticSyntax>() != null;
        }

        private static bool IsStatementContext(SyntaxTree syntaxTree, SourceLocation position, SyntaxToken tokenOnLeftOfPosition)
        {
            var token = tokenOnLeftOfPosition;
            token = token.GetPreviousTokenIfTouchingWord(position);

            return token.IsBeginningOfStatementContext();
        }
    }
}