using System.Collections.Generic;
using System.Linq;

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

        public static IEnumerable<SyntaxNodeBase> DescendantNodes(this SyntaxNodeBase root)
        {
            return root.DescendantNodesAndSelf().Skip(1);
        }

        public static IEnumerable<SyntaxNodeBase> DescendantNodesAndSelf(this SyntaxNodeBase root)
        {
            var stack = new Stack<SyntaxNodeBase>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current;

                foreach (var child in current.ChildNodes.Reverse())
                    stack.Push(child);
            }
        }
    }
}
