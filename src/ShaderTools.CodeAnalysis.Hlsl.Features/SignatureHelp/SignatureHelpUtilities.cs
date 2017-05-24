using System;
using System.Collections.Generic;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.SignatureHelp;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.SignatureHelp
{
    internal static class SignatureHelpUtilities
    {
        private static readonly Func<ArgumentListSyntax, SyntaxToken> s_getBaseArgumentListOpenToken = list => list.OpenParenToken;
        private static readonly Func<ArgumentListSyntax, SyntaxToken> s_getBaseArgumentListCloseToken = list => list.CloseParenToken;

        private static readonly Func<ArgumentListSyntax, IEnumerable<SyntaxNodeBase>> s_getBaseArgumentListArgumentsWithSeparators =
            list => list.Arguments.GetWithSeparators();

        private static readonly Func<ArgumentListSyntax, IEnumerable<string>> s_getBaseArgumentListNames =
            list => list.Arguments.Select(argument => (string) null);

        internal static SignatureHelpState GetSignatureHelpState(ArgumentListSyntax argumentList, SourceLocation position)
        {
            return GetSignatureHelpState(
                argumentList, position,
                s_getBaseArgumentListOpenToken,
                s_getBaseArgumentListCloseToken,
                s_getBaseArgumentListArgumentsWithSeparators,
                s_getBaseArgumentListNames);
        }

        internal static SignatureHelpState GetSignatureHelpState<TArgumentList>(
            TArgumentList argumentList,
            SourceLocation position,
            Func<TArgumentList, SyntaxToken> getOpenToken,
            Func<TArgumentList, SyntaxToken> getCloseToken,
            Func<TArgumentList, IEnumerable<SyntaxNodeBase>> getArgumentsWithSeparators,
            Func<TArgumentList, IEnumerable<string>> getArgumentNames)
            where TArgumentList : SyntaxNode
        {
            if (TryGetCurrentArgumentIndex(argumentList, position, getOpenToken, getCloseToken, getArgumentsWithSeparators, out var argumentIndex))
            {
                var argumentNames = getArgumentNames(argumentList).ToList();
                var argumentCount = argumentNames.Count;

                return new SignatureHelpState(
                    argumentIndex,
                    argumentCount,
                    argumentIndex < argumentNames.Count ? argumentNames[argumentIndex] : null,
                    argumentNames.Where(s => s != null).ToList());
            }

            return null;
        }

        private static bool TryGetCurrentArgumentIndex<TArgumentList>(
            TArgumentList argumentList,
            SourceLocation position,
            Func<TArgumentList, SyntaxToken> getOpenToken,
            Func<TArgumentList, SyntaxToken> getCloseToken,
            Func<TArgumentList, IEnumerable<SyntaxNodeBase>> getArgumentsWithSeparators,
            out int index) where TArgumentList : SyntaxNode
        {
            index = 0;
            if (position < getOpenToken(argumentList).SourceRange.End)
            {
                return false;
            }

            var closeToken = getCloseToken(argumentList);
            if (!closeToken.IsMissing &&
                position > closeToken.SourceRange.Start)
            {
                return false;
            }

            foreach (var element in getArgumentsWithSeparators(argumentList))
            {
                if (element.IsToken && position >= element.SourceRange.End)
                {
                    index++;
                }
            }

            return true;
        }

        internal static SourceRange GetSignatureHelpSpan(ArgumentListSyntax argumentList)
        {
            return GetSignatureHelpSpan(argumentList, s_getBaseArgumentListCloseToken);
        }

        internal static SourceRange GetSignatureHelpSpan<TArgumentList>(
            TArgumentList argumentList,
            Func<TArgumentList, SyntaxToken> getCloseToken)
            where TArgumentList : SyntaxNode
        {
            return GetSignatureHelpSpan(argumentList, argumentList.Parent.SourceRange.Start, getCloseToken);
        }

        internal static SourceRange GetSignatureHelpSpan<TArgumentList>(
            TArgumentList argumentList,
            SourceLocation start,
            Func<TArgumentList, SyntaxToken> getCloseToken)
            where TArgumentList : SyntaxNode
        {
            var closeToken = getCloseToken(argumentList);
            if (closeToken.RawKind != 0 && !closeToken.IsMissing)
            {
                return SourceRange.FromBounds(start, closeToken.SourceRange.Start);
            }

            // Missing close paren, the span is up to the start of the next token.
            var lastToken = argumentList.GetLastToken();
            var nextToken = lastToken.GetNextToken();
            if (nextToken == null)
            {
                nextToken = argumentList.AncestorsAndSelf().Last().GetLastToken(includeZeroLength: true);
            }

            return SourceRange.FromBounds(start, nextToken.SourceRange.Start);
        }
    }
}
