using System;
using System.Collections.Generic;
using System.Linq;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
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
                        var t = GetLastToken((SyntaxNode) nodeOrToken, tokenPredicate, triviaPredicate);
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
            if (node.Parent != null && !(node is StructuredTriviaSyntax))
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
                            var t = GetLastToken((SyntaxNode) nodeOrToken, tokenPredicate, triviaPredicate);
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

            var trivia = structuredTrivia.Parent as SyntaxToken;
            return trivia == null
                ? null
                : GetPreviousTokenFromTrivia(trivia, tokenPredicate, triviaPredicate);
        }

        private static SyntaxToken GetPreviousTokenFromTrivia(SyntaxToken token, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxNode, bool> triviaPredicate)
        {
            if (token == null)
                return null;

            var tt = GetPreviousToken(token.TrailingTrivia, token, tokenPredicate, triviaPredicate);
            if (tt != null)
                return tt;

            var t = GetPreviousToken(token, false, tokenPredicate, triviaPredicate);
            if (t != null)
                return t;

            var lt = GetPreviousToken(token.LeadingTrivia, token, tokenPredicate, triviaPredicate);
            if (lt != null)
                return lt;

            return null;
        }

        private static SyntaxToken GetPreviousToken(IEnumerable<SyntaxNode> triviaList, SyntaxToken parentToken, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxNode, bool> triviaPredicate)
        {
            var returnNext = false;

            foreach (var otherTrivia in triviaList.Reverse())
            {
                if (returnNext)
                {
                    var structure = otherTrivia as StructuredTriviaSyntax;
                    if (triviaPredicate(otherTrivia) && structure != null)
                    {
                        var token = GetLastToken(structure, tokenPredicate, triviaPredicate);
                        if (token != null)
                            return token;
                    }
                }
                else if (otherTrivia == parentToken)
                {
                    returnNext = true;
                }
            }

            var isTrailing = ReferenceEquals(triviaList, parentToken.TrailingTrivia);
            if (returnNext && isTrailing && tokenPredicate(parentToken))
                return parentToken;

            return null;
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
                        var t = GetFirstToken((SyntaxNode) nodeOrToken, tokenPredicate, triviaPredicate);
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
                            var t = GetFirstToken((SyntaxNode) nodeOrToken, tokenPredicate, triviaPredicate);
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

        public static SyntaxToken GetLastToken(SyntaxNode node, bool includeZeroLength, bool includeSkippedTokens)
        {
            var tokenPredicate = GetTokenPredicate(includeZeroLength);
            var triviaPredicate = GetTriviaPredicate(includeSkippedTokens);
            return GetLastToken(node, tokenPredicate, triviaPredicate);
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
                    var t = GetFirstToken((SyntaxNode) nodeOrToken, tokenPredicate, triviaPredicate);
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
                    var t = GetLastToken((SyntaxNode) nodeOrToken, tokenPredicate, triviaPredicate);
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