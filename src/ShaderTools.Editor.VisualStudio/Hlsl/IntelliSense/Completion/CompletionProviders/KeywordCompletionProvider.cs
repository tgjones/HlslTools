using System.Collections.Generic;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Editor.VisualStudio.Core.Glyphs;
using ShaderTools.Editor.VisualStudio.Hlsl.Util.ContextQuery;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.Completion.CompletionProviders
{
    //[Export(typeof(ICompletionProvider))]
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
            var isInNonUserCode = ((SyntaxNode) syntaxTree.Root).InNonUserCode(position);
            if (isInNonUserCode)
                yield break;

            var isPreprocessorDirectiveContext = syntaxTree.DefinitelyInMacro(position);

            var leftToken = ((SyntaxNode) syntaxTree.Root).FindTokenOnLeft(position);

            var targetToken = leftToken.GetPreviousTokenIfTouchingWord(position);
            if (targetToken == null)
                yield break;

            var isPreprocessorKeywordContext = isPreprocessorDirectiveContext && syntaxTree.IsPreprocessorKeywordContext(position, leftToken);

            var isStatementContext = !isPreprocessorDirectiveContext && targetToken.IsBeginningOfStatementContext();

            var isSemanticContext = !isPreprocessorDirectiveContext && leftToken.HasAncestor<SemanticSyntax>();

            var isTypeDeclarationContext = syntaxTree.IsTypeDeclarationContext(targetToken);

            if (IsValidBreakKeywordContext(isStatementContext, leftToken))
                yield return SyntaxKind.BreakKeyword;

            if (targetToken.IsSwitchLabelContext())
                yield return SyntaxKind.CaseKeyword;

            if (IsValidContinueKeywordContext(isStatementContext, leftToken))
                yield return SyntaxKind.ContinueKeyword;

            if (isPreprocessorDirectiveContext || IsValidElseKeywordContext(targetToken))
                yield return SyntaxKind.ElseKeyword;

            if (isPreprocessorKeywordContext || isStatementContext)
                yield return SyntaxKind.IfKeyword;

            if (isSemanticContext)
                yield return SyntaxKind.PackoffsetKeyword;

            if (isStatementContext)
                yield return SyntaxKind.ReturnKeyword;

            if (isSemanticContext)
                yield return SyntaxKind.RegisterKeyword;

            if (isTypeDeclarationContext)
                yield return SyntaxKind.StructKeyword;

            if (isStatementContext)
                yield return SyntaxKind.SwitchKeyword;

            if (isStatementContext || IsValidWhileKeywordContext(targetToken))
                yield return SyntaxKind.WhileKeyword;
        }

        private static bool IsValidBreakKeywordContext(bool isStatementContext, SyntaxToken token)
        {
            if (!isStatementContext)
                return false;

            foreach (var v in token.Ancestors())
                if (v.IsBreakableConstruct())
                    return true;

            return false;
        }

        private static bool IsValidContinueKeywordContext(bool isStatementContext, SyntaxToken token)
        {
            if (!isStatementContext)
                return false;

            foreach (var v in token.Ancestors())
                if (v.IsContinuableConstruct())
                    return true;

            return false;
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
            if (token.IsKind(SyntaxKind.SemiToken) && ifStatement.Statement.GetLastToken(includeSkippedTokens: true) == token)
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

        private static bool IsValidWhileKeywordContext(SyntaxToken token)
        {
            // do {
            // } |

            // do {
            // } w|

            // Note: the case of
            //   do 
            //     Foo();
            //   |
            // is taken care of in the IsStatementContext case.

            if (token.Kind == SyntaxKind.CloseBraceToken &&
                token.Parent.IsKind(SyntaxKind.Block) &&
                token.Parent.IsParentKind(SyntaxKind.DoStatement))
            {
                return true;
            }

            return false;
        }
    }
}