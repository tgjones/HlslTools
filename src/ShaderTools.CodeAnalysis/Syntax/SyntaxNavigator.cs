using System;
using System.Collections.Generic;
using System.Linq;

namespace ShaderTools.CodeAnalysis.Syntax
{
    internal sealed class SyntaxNavigator
    {
        //public static readonly SyntaxNavigator Instance = new SyntaxNavigator();

        //private SyntaxNavigator()
        //{
        //}

        private static readonly Func<ISyntaxToken, bool> AnyTokenPredicate = t => true;
        private static readonly Func<ISyntaxToken, bool> NonZeroLengthTokenPredicate = t => t.SourceRange.Length > 0;

        private static readonly Func<SyntaxNodeBase, bool> NoTriviaPredicate = t => false;
        private static readonly Func<SyntaxNodeBase, bool> SkippedTokensTriviaPredicate = t => t.IsSkippedTokensTrivia;

        private static Func<ISyntaxToken, bool> GetTokenPredicate(bool includeZeroLength)
        {
            return includeZeroLength ? AnyTokenPredicate : NonZeroLengthTokenPredicate;
        }

        private static Func<SyntaxNodeBase, bool> GetTriviaPredicate(bool includeSkippedTokens)
        {
            return includeSkippedTokens ? SkippedTokensTriviaPredicate : NoTriviaPredicate;
        }

        public static ISyntaxToken GetPreviousToken(ISyntaxToken token, bool includeZeroLength, bool includeSkippedTokens)
        {
            var tokenPredicate = GetTokenPredicate(includeZeroLength);
            var triviaPredicate = GetTriviaPredicate(includeSkippedTokens);
            return GetPreviousToken(token, true, tokenPredicate, triviaPredicate);
        }

        private static ISyntaxToken GetPreviousToken(ISyntaxToken token, bool searchLeadingTrivia, Func<ISyntaxToken, bool> tokenPredicate, Func<SyntaxNodeBase, bool> triviaPredicate)
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
                        var t = GetLastToken((ISyntaxToken)nodeOrToken, tokenPredicate, triviaPredicate);
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

        private static ISyntaxToken GetPreviousToken(SyntaxNodeBase node, Func<ISyntaxToken, bool> tokenPredicate, Func<SyntaxNodeBase, bool> triviaPredicate)
        {
            if (node.Parent != null && !node.IsStructuredTrivia)
            {
                var returnNext = false;

                foreach (var nodeOrToken in node.Parent.ChildNodes.Reverse())
                {
                    if (returnNext)
                    {
                        if (nodeOrToken.IsToken)
                        {
                            var t = GetLastToken((ISyntaxToken)nodeOrToken, tokenPredicate, triviaPredicate);
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

            if (!node.IsStructuredTrivia)
                return null;

            var trivia = node.Parent as ISyntaxToken;
            return trivia == null
                ? null
                : GetPreviousTokenFromTrivia(trivia, tokenPredicate, triviaPredicate);
        }

        private static ISyntaxToken GetPreviousTokenFromTrivia(ISyntaxToken token, Func<ISyntaxToken, bool> tokenPredicate, Func<SyntaxNodeBase, bool> triviaPredicate)
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

        private static ISyntaxToken GetPreviousToken(IEnumerable<SyntaxNodeBase> triviaList, ISyntaxToken parentToken, Func<ISyntaxToken, bool> tokenPredicate, Func<SyntaxNodeBase, bool> triviaPredicate)
        {
            var returnNext = false;

            foreach (var otherTrivia in triviaList.Reverse())
            {
                if (returnNext)
                {
                    if (triviaPredicate(otherTrivia) && otherTrivia.IsStructuredTrivia)
                    {
                        var token = GetLastToken(otherTrivia, tokenPredicate, triviaPredicate);
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

        public static ISyntaxToken GetNextToken(ISyntaxToken token, bool includeZeroLength, bool includeSkippedTokens)
        {
            var tokenPredicate = GetTokenPredicate(includeZeroLength);
            var triviaPredicate = GetTriviaPredicate(includeSkippedTokens);
            return GetNextToken(token, true, tokenPredicate, triviaPredicate);
        }

        private static ISyntaxToken GetNextToken(ISyntaxToken token, bool searchTrailingTrivia, Func<ISyntaxToken, bool> tokenPredicate, Func<SyntaxNodeBase, bool> triviaPredicate)
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
                        var t = GetFirstToken((ISyntaxToken)nodeOrToken, tokenPredicate, triviaPredicate);
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

        private static ISyntaxToken GetNextToken(SyntaxNodeBase node, Func<ISyntaxToken, bool> tokenPredicate, Func<SyntaxNodeBase, bool> triviaPredicate)
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
                            var t = GetFirstToken((ISyntaxToken)nodeOrToken, tokenPredicate, triviaPredicate);
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

        public static ISyntaxToken GetFirstToken(SyntaxNodeBase node, bool includeZeroLength, bool includeSkippedTokens)
        {
            var tokenPredicate = GetTokenPredicate(includeZeroLength);
            var triviaPredicate = GetTriviaPredicate(includeSkippedTokens);
            return GetFirstToken(node, tokenPredicate, triviaPredicate);
        }

        public static ISyntaxToken GetLastToken(SyntaxNodeBase node, bool includeZeroLength, bool includeSkippedTokens)
        {
            var tokenPredicate = GetTokenPredicate(includeZeroLength);
            var triviaPredicate = GetTriviaPredicate(includeSkippedTokens);
            return GetLastToken(node, tokenPredicate, triviaPredicate);
        }

        private static ISyntaxToken GetFirstToken(SyntaxNodeBase node, Func<ISyntaxToken, bool> tokenPredicate, Func<SyntaxNodeBase, bool> triviaPredicate)
        {
            foreach (var nodeOrToken in node.ChildNodes)
            {
                if (nodeOrToken.IsToken)
                {
                    var t = GetFirstToken((ISyntaxToken)nodeOrToken, tokenPredicate, triviaPredicate);
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

        private static ISyntaxToken GetFirstToken(ISyntaxToken token, Func<ISyntaxToken, bool> tokenPredicate, Func<SyntaxNodeBase, bool> triviaPredicate)
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

        private static ISyntaxToken GetFirstToken(IEnumerable<SyntaxNodeBase> triviaList, Func<ISyntaxToken, bool> tokenPredicate, Func<SyntaxNodeBase, bool> triviaPredicate)
        {
            foreach (var trivia in triviaList)
            {
                if (triviaPredicate(trivia) && trivia.IsStructuredTrivia)
                {
                    var t = GetFirstToken(trivia, tokenPredicate, triviaPredicate);
                    if (t != null)
                        return t;
                }
            }

            return null;
        }

        private static ISyntaxToken GetLastToken(SyntaxNodeBase node, Func<ISyntaxToken, bool> tokenPredicate, Func<SyntaxNodeBase, bool> triviaPredicate)
        {
            foreach (var nodeOrToken in node.ChildNodes.Reverse())
            {
                if (nodeOrToken.IsToken)
                {
                    var t = GetLastToken((ISyntaxToken)nodeOrToken, tokenPredicate, triviaPredicate);
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

        private static ISyntaxToken GetLastToken(ISyntaxToken token, Func<ISyntaxToken, bool> tokenPredicate, Func<SyntaxNodeBase, bool> triviaPredicate)
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

        private static ISyntaxToken GetLastToken(IEnumerable<SyntaxNodeBase> triviaList, Func<ISyntaxToken, bool> tokenPredicate, Func<SyntaxNodeBase, bool> triviaPredicate)
        {
            foreach (var trivia in triviaList.Reverse())
            {
                if (triviaPredicate(trivia) && trivia.IsStructuredTrivia)
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
