using HlslTools.Syntax;

namespace HlslTools.VisualStudio.Util.ContextQuery
{
    internal static class SyntaxTreeExtensions
    {
        public static bool IsPreprocessorKeywordContext(this SyntaxTree syntaxTree, SourceLocation position, SyntaxToken preProcessorTokenOnLeftOfPosition)
        {
            // cases:
            //  #|
            //  #d|
            //  # |
            //  # d|

            // note: comments are not allowed between the # and item.
            var token = preProcessorTokenOnLeftOfPosition;
            token = token.GetPreviousTokenIfTouchingWord(position);

            if (token.IsKind(SyntaxKind.HashToken))
                return true;

            return false;
        }
    }
}