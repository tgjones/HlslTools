using System;
using System.Collections.Generic;
using System.Linq;

namespace HlslTools.Syntax
{
    internal static class SyntaxTreeNavigation
    {
        private static readonly Func<SyntaxToken, bool> AnyTokenPredicate = t => true;
        private static readonly Func<SyntaxToken, bool> NonZeroLengthTokenPredicate = t => t.SourceRange.Length > 0;

        private static readonly Func<SyntaxNode, bool> NoTriviaPredicate = t => false;
        private static readonly Func<SyntaxNode, bool> SkippedTokensTriviaPredicate = t => t.Kind == SyntaxKind.SkippedTokensTrivia;

        private static Func<SyntaxToken, bool> GetTokenPredicate(bool includeZeroLength)
        {
            return includeZeroLength ? AnyTokenPredicate : NonZeroLengthTokenPredicate;
        }

        private static Func<SyntaxNode, bool> GetTriviaPredicate(bool includeSkippedTokens)
        {
            return includeSkippedTokens ? SkippedTokensTriviaPredicate : NoTriviaPredicate;
        }

        public static SyntaxToken GetPreviousToken(SyntaxToken token, bool includeZeroLength, bool includeSkippedTokens)
        {
            var tokenPredicate = GetTokenPredicate(includeZeroLength);
            var triviaPredicate = GetTriviaPredicate(includeSkippedTokens);
            return GetPreviousToken(token, true, tokenPredicate, triviaPredicate);
        }

        private static SyntaxToken GetPreviousToken(SyntaxToken token, bool searchLeadingTrivia, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxNode, bool> triviaPredicate)
        {
            if (searchLeadingTrivia)
            {
                var lt = GetLastToken(token.LeadingTrivia, tokenPredicate, triviaPredicate);
                if (lt != null)
                    return lt;
            }

            if (token.Parent == null)
                return null;

            var returnNext = false;

            foreach (var nodeOrToken in token.Parent.ChildNodes.Reverse())
            {
                if (returnNext)
                {
                    if (nodeOrToken.IsToken)
                    {
                        var t = GetLastToken((SyntaxToken) nodeOrToken, tokenPredicate, triviaPredicate);
                        if (t != null)
                            return t;
                    }
                    else
                    {
                        var t = GetLastToken(nodeOrToken, tokenPredicate, triviaPredicate);
                        if (t != null)
                            return t;
                    }

                }
                else if (nodeOrToken.IsToken && nodeOrToken == token)
                {
                    returnNext = true;
                }
            }

            return GetPreviousToken(token.Parent, tokenPredicate, triviaPredicate);
        }

        private static SyntaxToken GetPreviousToken(SyntaxNode node, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxNode, bool> triviaPredicate)
        {
            if (node.Parent != null)
            {
                var returnNext = false;

                foreach (var nodeOrToken in node.Parent.ChildNodes.Reverse())
                {
                    if (returnNext)
                    {
                        if (nodeOrToken.IsToken)
                        {
                            var t = GetLastToken((SyntaxToken) nodeOrToken, tokenPredicate, triviaPredicate);
                            if (t != null)
                                return t;
                        }
                        else
                        {
                            var t = GetLastToken(nodeOrToken, tokenPredicate, triviaPredicate);
                            if (t != null)
                                return t;
                        }
                    }

                    if (!nodeOrToken.IsToken && nodeOrToken == node)
                        returnNext = true;
                }

                return GetPreviousToken(node.Parent, tokenPredicate, triviaPredicate);
            }

            var structuredTrivia = node as StructuredTriviaSyntax;
            if (structuredTrivia == null)
                return null;

            var trivia = structuredTrivia.Parent;
            return trivia == null
                ? null
                : GetPreviousToken(trivia, tokenPredicate, triviaPredicate);
        }

        public static SyntaxToken GetNextToken(SyntaxToken token, bool includeZeroLength, bool includeSkippedTokens)
        {
            var tokenPredicate = GetTokenPredicate(includeZeroLength);
            var triviaPredicate = GetTriviaPredicate(includeSkippedTokens);
            return GetNextToken(token, true, tokenPredicate, triviaPredicate);
        }

        private static SyntaxToken GetNextToken(SyntaxToken token, bool searchTrailingTrivia, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxNode, bool> triviaPredicate)
        {
            if (searchTrailingTrivia)
            {
                var tt = GetFirstToken(token.TrailingTrivia, tokenPredicate, triviaPredicate);
                if (tt != null)
                    return tt;
            }

            if (token.Parent == null)
                return null;

            var returnNext = false;

            foreach (var nodeOrToken in token.Parent.ChildNodes)
            {
                if (returnNext)
                {
                    if (nodeOrToken.IsToken)
                    {
                        var t = GetFirstToken((SyntaxToken) nodeOrToken, tokenPredicate, triviaPredicate);
                        if (t != null)
                            return t;
                    }
                    else
                    {
                        var t = GetFirstToken(nodeOrToken, tokenPredicate, triviaPredicate);
                        if (t != null)
                            return t;
                    }

                }
                else if (nodeOrToken.IsToken && nodeOrToken == token)
                {
                    returnNext = true;
                }
            }

            return GetNextToken(token.Parent, tokenPredicate, triviaPredicate);
        }

        private static SyntaxToken GetNextToken(SyntaxNode node, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxNode, bool> triviaPredicate)
        {
            if (node.Parent != null)
            {
                var returnNext = false;

                foreach (var nodeOrToken in node.Parent.ChildNodes)
                {
                    if (returnNext)
                    {
                        if (nodeOrToken.IsToken)
                        {
                            var t = GetFirstToken((SyntaxToken) nodeOrToken, tokenPredicate, triviaPredicate);
                            if (t != null)
                                return t;
                        }
                        else
                        {
                            var t = GetFirstToken(nodeOrToken, tokenPredicate, triviaPredicate);
                            if (t != null)
                                return t;
                        }
                    }

                    if (!nodeOrToken.IsToken && nodeOrToken == node)
                        returnNext = true;
                }

                return GetNextToken(node.Parent, tokenPredicate, triviaPredicate);
            }

            return null;
        }

        public static SyntaxToken GetFirstToken(SyntaxNode node, bool includeZeroLength, bool includeSkippedTokens)
        {
            var tokenPredicate = GetTokenPredicate(includeZeroLength);
            var triviaPredicate = GetTriviaPredicate(includeSkippedTokens);
            return GetFirstToken(node, tokenPredicate, triviaPredicate);
        }

        private static SyntaxToken GetFirstToken(SyntaxNode node, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxNode, bool> triviaPredicate)
        {
            foreach (var nodeOrToken in node.ChildNodes)
            {
                if (nodeOrToken.IsToken)
                {
                    var t = GetFirstToken((SyntaxToken) nodeOrToken, tokenPredicate, triviaPredicate);
                    if (t != null)
                        return t;
                }
                else
                {
                    var t = GetFirstToken(nodeOrToken, tokenPredicate, triviaPredicate);
                    if (t != null)
                        return t;
                }
            }

            return null;
        }

        private static SyntaxToken GetFirstToken(SyntaxToken token, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxNode, bool> triviaPredicate)
        {
            var lt = GetFirstToken(token.LeadingTrivia, tokenPredicate, triviaPredicate);
            if (lt != null)
                return lt;

            if (tokenPredicate(token))
                return token;

            var tt = GetFirstToken(token.TrailingTrivia, tokenPredicate, triviaPredicate);
            if (tt != null)
                return tt;

            return null;
        }

        private static SyntaxToken GetFirstToken(IEnumerable<SyntaxNode> triviaList, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxNode, bool> triviaPredicate)
        {
            foreach (var trivia in triviaList)
            {
                if (triviaPredicate(trivia) && trivia is StructuredTriviaSyntax)
                {
                    var t = GetFirstToken(trivia, tokenPredicate, triviaPredicate);
                    if (t != null)
                        return t;
                }
            }

            return null;
        }

        private static SyntaxToken GetLastToken(SyntaxNode node, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxNode, bool> triviaPredicate)
        {
            foreach (var nodeOrToken in node.ChildNodes.Reverse())
            {
                if (nodeOrToken.IsToken)
                {
                    var t = GetLastToken((SyntaxToken) nodeOrToken, tokenPredicate, triviaPredicate);
                    if (t != null)
                        return t;
                }
                else
                {
                    var t = GetLastToken(nodeOrToken, tokenPredicate, triviaPredicate);
                    if (t != null)
                        return t;
                }
            }

            return null;
        }

        private static SyntaxToken GetLastToken(SyntaxToken token, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxNode, bool> triviaPredicate)
        {
            var tt = GetLastToken(token.TrailingTrivia, tokenPredicate, triviaPredicate);
            if (tt != null)
                return tt;

            if (tokenPredicate(token))
                return token;

            var lt = GetLastToken(token.LeadingTrivia, tokenPredicate, triviaPredicate);
            if (lt != null)
                return lt;

            return null;
        }

        private static SyntaxToken GetLastToken(IEnumerable<SyntaxNode> triviaList, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxNode, bool> triviaPredicate)
        {
            foreach (var trivia in triviaList.Reverse())
            {
                if (triviaPredicate(trivia) && trivia is StructuredTriviaSyntax)
                {
                    var t = GetLastToken(trivia, tokenPredicate, triviaPredicate);
                    if (t != null)
                        return t;
                }
            }

            return null;
        }
    }
}