using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Completion;
using ShaderTools.CodeAnalysis.Hlsl.Completion.Providers;
using ShaderTools.CodeAnalysis.Hlsl.Extensions.ContextQuery;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Shared.Extensions;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Completion.CompletionProviders
{
    internal sealed class KeywordCompletionProvider : CompletionProvider
    {
        internal override bool IsInsertionTrigger(SourceText text, int insertedCharacterPosition, OptionSet options)
        {
            return CompletionUtilities.IsTriggerCharacter(text, insertedCharacterPosition, options);
        }

        public override async Task ProvideCompletionsAsync(CompletionContext context)
        {
            var syntaxTree = (SyntaxTree) await context.Document.GetSyntaxTreeAsync(context.CancellationToken).ConfigureAwait(false);

            var sourceLocation = syntaxTree.MapRootFilePosition(context.Position);

            var availableKeywords = GetAvailableKeywords(syntaxTree, sourceLocation);

            foreach (var keyword in availableKeywords)
            {
                var keywordText = keyword.GetText();

                context.AddItem(CommonCompletionItem.Create(
                    keywordText,
                    Glyph.Keyword,
                    (keywordText + " Keyword").ToSymbolMarkupTokens()));
            }
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
                targetToken = new SyntaxToken(SyntaxKind.None, true, new SourceRange(), new SourceFileSpan());

            var isPreprocessorKeywordContext = isPreprocessorDirectiveContext && syntaxTree.IsPreprocessorKeywordContext(position, leftToken);

            var isStatementContext = !isPreprocessorDirectiveContext && targetToken.IsBeginningOfStatementContext();

            var isSemanticContext = !isPreprocessorDirectiveContext && leftToken.HasAncestor<SemanticSyntax>();

            var isBooleanLiteralExpressionContext = leftToken.HasAncestor<ExpressionSyntax>() && SymbolCompletionProvider.GetPropertyAccessExpression((SyntaxNode)syntaxTree.Root, position) == null;

            var isTypeDeclarationContext = syntaxTree.IsTypeDeclarationContext(targetToken);

            if (IsValidBreakKeywordContext(isStatementContext, leftToken))
                yield return SyntaxKind.BreakKeyword;

            if (targetToken.IsSwitchLabelContext())
                yield return SyntaxKind.CaseKeyword;

            if (isStatementContext)
                yield return SyntaxKind.ConstKeyword;

            if (IsValidContinueKeywordContext(isStatementContext, leftToken))
                yield return SyntaxKind.ContinueKeyword;

            if (isStatementContext)
                yield return SyntaxKind.DoKeyword;

            if (isPreprocessorDirectiveContext || IsValidElseKeywordContext(targetToken))
                yield return SyntaxKind.ElseKeyword;

            if (isPreprocessorKeywordContext || isStatementContext)
                yield return SyntaxKind.IfKeyword;

            if (isStatementContext || isBooleanLiteralExpressionContext)
                yield return SyntaxKind.FalseKeyword;

            if (isStatementContext)
                yield return SyntaxKind.ForKeyword;

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

            if (isStatementContext || isBooleanLiteralExpressionContext)
                yield return SyntaxKind.TrueKeyword;

            if (isStatementContext || IsValidWhileKeywordContext(targetToken))
                yield return SyntaxKind.WhileKeyword;
        }

        private static bool IsValidBreakKeywordContext(bool isStatementContext, SyntaxToken token)
        {
            if (!isStatementContext)
                return false;

            foreach (var v in token.Ancestors().Cast<SyntaxNode>())
                if (v.IsBreakableConstruct())
                    return true;

            return false;
        }

        private static bool IsValidContinueKeywordContext(bool isStatementContext, SyntaxToken token)
        {
            if (!isStatementContext)
                return false;

            foreach (var v in token.Ancestors().Cast<SyntaxNode>())
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
