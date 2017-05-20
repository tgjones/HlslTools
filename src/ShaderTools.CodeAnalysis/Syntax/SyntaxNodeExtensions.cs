namespace ShaderTools.CodeAnalysis.Syntax
{
    public static class SyntaxNodeExtensions
    {
        public static ISyntaxToken GetPreviousToken(this ISyntaxToken token, bool includeZeroLength = false, bool includeSkippedTokens = false)
        {
            return SyntaxNavigator.GetPreviousToken(token, includeZeroLength, includeSkippedTokens);
        }

        public static ISyntaxToken GetNextToken(this ISyntaxToken token, bool includeZeroLength = false, bool includeSkippedTokens = false)
        {
            return SyntaxNavigator.GetNextToken(token, includeZeroLength, includeSkippedTokens);
        }

        public static ISyntaxToken GetFirstToken(this SyntaxNodeBase token, bool includeZeroLength = false, bool includeSkippedTokens = false)
        {
            return SyntaxNavigator.GetFirstToken(token, includeZeroLength, includeSkippedTokens);
        }

        public static ISyntaxToken GetLastToken(this SyntaxNodeBase token, bool includeZeroLength = false, bool includeSkippedTokens = false)
        {
            return SyntaxNavigator.GetLastToken(token, includeZeroLength, includeSkippedTokens);
        }
    }
}
