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
            var isInNonUserCode = syntaxTree.Root.InNonUserCode(position);
            if (isInNonUserCode)
                yield break;

            var isPreprocessorDirectiveContext = syntaxTree.DefinitelyInMacro(position);

            var leftToken = syntaxTree.Root.FindTokenOnLeft(position);

            var targetToken = leftToken.GetPreviousTokenIfTouchingWord(position);

            var isPreprocessorKeywordContext = isPreprocessorDirectiveContext && syntaxTree.IsPreprocessorKeywordContext(position, leftToken);

            var isStatementContext = !isPreprocessorDirectiveContext && targetToken.IsBeginningOfStatementContext();

            var isSemanticContext = !isPreprocessorDirectiveContext && leftToken.HasAncestor<SemanticSyntax>();

            if (isStatementContext)
            {
                yield return SyntaxKind.ReturnKeyword;
            }

            if (isPreprocessorKeywordContext || isStatementContext)
            {
                yield return SyntaxKind.IfKeyword;
            }

            if (isPreprocessorDirectiveContext || IsValidElseKeywordContext(targetToken))
            {
                yield return SyntaxKind.ElseKeyword;
            }

            if (isSemanticContext)
            {
                yield return SyntaxKind.PackoffsetKeyword;
                yield return SyntaxKind.RegisterKeyword;
            }
        }

        private static bool IsValidElseKeywordContext(SyntaxToken token)
        {
            var statement = token.GetAncestor<StatementSyntax>();
            var ifStatement = statement.GetAncestorOrThis<IfStatementSyntax>();

            if (statement == null || ifStatement == null)
                return false;

            // cases:
            //   if (foo)
            //     Console.WriteLine();
            //   |
            //   if (foo)
            //     Console.WriteLine();
            //   e|
            if (token.IsKind(SyntaxKind.SemiToken) && ifStatement.Statement.GetLastToken() == token)
                return true;

            // if (foo) {
            //     Console.WriteLine();
            //   } |
            //   if (foo) {
            //     Console.WriteLine();
            //   } e|
            if (token.IsKind(SyntaxKind.CloseBraceToken) && ifStatement.Statement is BlockSyntax && token == ((BlockSyntax) ifStatement.Statement).CloseBraceToken)
                return true;

            return false;
        }
    }
}