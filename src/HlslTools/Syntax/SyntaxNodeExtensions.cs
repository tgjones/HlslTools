using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using HlslTools.Diagnostics;
using HlslTools.Text;

namespace HlslTools.Syntax
{
    public static class SyntaxNodeExtensions
    {
        public static IEnumerable<SyntaxNode> Ancestors(this SyntaxNode node)
        {
            return node.AncestorsAndSelf().Skip(1);
        }

        public static IEnumerable<SyntaxNode> AncestorsAndSelf(this SyntaxNode node)
        {
            while (node != null)
            {
                yield return node;
                node = node.Parent;
            }
        }

        public static IEnumerable<SyntaxToken> DescendantTokens(this SyntaxNode node)
        {
            foreach (var childNode in node.ChildNodes)
            {
                if (childNode.IsToken)
                    yield return (SyntaxToken) childNode;
                else
                    foreach (var descendantToken in childNode.DescendantTokens())
                        yield return descendantToken;
            }
        }

        public static IEnumerable<SyntaxToken> DescendantTokensReverse(this SyntaxNode node)
        {
            foreach (var childNode in node.ChildNodes.Reverse())
            {
                if (childNode.IsToken)
                    yield return (SyntaxToken) childNode;
                else
                    foreach (var descendantToken in childNode.DescendantTokensReverse())
                        yield return descendantToken;
            }
        }

        public static TextSpan GetTextSpan(this SyntaxNode node)
        {
            var firstToken = node.GetFirstTokenInDescendants();
            if (firstToken == null)
                return TextSpan.None;

            var lastToken = node.GetLastTokenInDescendants();
            if (lastToken == null)
                return TextSpan.None;

#if DEBUG
            var tokens = node.DescendantTokens().ToList();
            if (!tokens.Any())
                throw new ArgumentException();

            var filename = tokens[0].Span.Filename;
            if (tokens.Skip(1).Any(x => x.Span.Filename != filename))
                throw new ArgumentException("GetTextSpan cannot be called for nodes that span more than one file.");
#endif

            return TextSpan.FromBounds(
                firstToken.Span.Filename,
                firstToken.Span.Start, 
                lastToken.Span.End);
        }

        public static TextSpan GetTextSpanSafe(this SyntaxNode node)
        {
            if (node is LocatedNode)
                return ((LocatedNode) node).Span;

            var firstToken = node.GetFirstTokenInDescendants();
            if (firstToken == null)
                return TextSpan.None;

            var lastToken = node.GetLastTokenInDescendants(t => t.Span.Filename == firstToken.Span.Filename);
            if (lastToken == null)
                return TextSpan.None;

            return TextSpan.FromBounds(
                firstToken.Span.Filename,
                firstToken.Span.Start,
                lastToken.Span.End);
        }

        public static TextSpan GetTextSpanRoot(this SyntaxNode node)
        {
            if (node is LocatedNode)
                return ((LocatedNode) node).Span;

            var firstToken = node.GetFirstTokenInDescendants(t => t.Span.IsInRootFile);
            if (firstToken == null)
                return TextSpan.None;

            var lastToken = node.GetLastTokenInDescendants(t => t.Span.IsInRootFile);
            if (lastToken == null)
                return TextSpan.None;

            return TextSpan.FromBounds(
                null,
                firstToken.Span.Start,
                lastToken.Span.End);
        }

        public static SyntaxToken GetLastToken(this SyntaxNode node)
        {
            return node.ChildNodes.LastOrDefault(n => n.IsToken) as SyntaxToken;
        }

        public static SyntaxToken GetLastTokenInDescendants(this SyntaxNode node, Func<SyntaxToken, bool> filter = null)
        {
            return node.DescendantTokensReverse().FirstOrDefault(filter ?? (t => true));
        }

        public static SyntaxToken GetFirstTokenInDescendants(this SyntaxNode node, Func<SyntaxToken, bool> filter = null)
        {
            return node.DescendantTokens().FirstOrDefault(filter ?? (t => true));
        }

        public static TextSpan GetLastSpanIncludingTrivia(this SyntaxToken node)
        {
            var lastTrailingLocatedNode = node.TrailingTrivia.OfType<LocatedNode>().LastOrDefault();
            if (lastTrailingLocatedNode != null)
                return lastTrailingLocatedNode.Span;
            return node.Span;
        }

        public static SyntaxToken GetFirstToken(this SyntaxNode node)
        {
            return node.ChildNodes.FirstOrDefault(n => n.IsToken) as SyntaxToken;
        }

        public static SyntaxToken FindTokenOnLeft(this SyntaxNode root, SourceLocation position)
        {
            var token = root.FindToken(position, descendIntoTrivia: true);
            return token.GetPreviousTokenIfTouchingEndOrCurrentIsEndOfFile(position);
        }

        private static SyntaxToken GetPreviousTokenIfTouchingEndOrCurrentIsEndOfFile(this SyntaxToken token, SourceLocation position)
        {
            var previous = token.GetPreviousToken(includeZeroLength: false, includeSkippedTokens: true);
            if (previous != null)
            {
                if (token.Kind == SyntaxKind.EndOfFileToken || previous.SourceRange.End == position)
                    return previous;
            }

            return token;
        }

        public static SyntaxToken GetPreviousToken(this SyntaxToken token, bool includeZeroLength = false, bool includeSkippedTokens = false)
        {
            return SyntaxTreeNavigation.GetPreviousToken(token, includeZeroLength, includeSkippedTokens);
        }

        public static SyntaxToken GetNextToken(this SyntaxToken token, bool includeZeroLength = false, bool includeSkippedTokens = false)
        {
            return SyntaxTreeNavigation.GetNextToken(token, includeZeroLength, includeSkippedTokens);
        }

        public static SyntaxToken FindTokenContext(this SyntaxNode root, SourceLocation position)
        {
            var token = root.FindTokenOnLeft(position);

            // In case the previous or next token is a missing token, we'll use this
            // one instead.

            if (!token.SourceRange.ContainsOrTouches(position))
            {
                // token <missing> | token
                var previousToken = token.GetPreviousToken(includeZeroLength: true);
                if (previousToken != null && previousToken.IsMissing && previousToken.SourceRange.End <= position)
                    return previousToken;

                // token | <missing> token
                var nextToken = token.GetNextToken(includeZeroLength: true);
                if (nextToken != null && nextToken.IsMissing && position <= nextToken.SourceRange.Start)
                    return nextToken;
            }

            return token;
        }

        public static bool InComment(this SyntaxNode root, SourceLocation position)
        {
            var token = root.FindTokenOnLeft(position);
            return (from t in token.LeadingTrivia.Concat(token.TrailingTrivia)
                where t.SourceRange.ContainsOrTouches(position)
                where t.Kind == SyntaxKind.SingleLineCommentTrivia ||
                      t.Kind == SyntaxKind.MultiLineCommentTrivia
                select t).Any();
        }

        public static bool InLiteral(this SyntaxNode root, SourceLocation position)
        {
            var token = root.FindTokenOnLeft(position);
            return token.SourceRange.ContainsOrTouches(position) && token.Kind.IsLiteral();
        }

        public static IEnumerable<SyntaxToken> FindStartTokens(this SyntaxNode root, SourceLocation position, bool descendIntoTriva = false)
        {
            var token = root.FindToken(position, descendIntoTriva);
            yield return token;

            var previousToken = token.GetPreviousToken();
            if (previousToken != null && previousToken.SourceRange.End == position)
                yield return previousToken;
        }

        public static TNode WithDiagnostics<TNode>(this TNode node, IEnumerable<Diagnostic> diagnostics)
            where TNode : SyntaxNode
        {
            return (TNode) node.SetDiagnostics(node.Diagnostics.AddRange(diagnostics));
        }

        public static TNode WithDiagnostic<TNode>(this TNode node, Diagnostic diagnostic)
            where TNode : SyntaxNode
        {
            return (TNode)node.SetDiagnostics(node.Diagnostics.Add(diagnostic));
        }

        public static TNode WithoutDiagnostics<TNode>(this TNode node)
            where TNode : SyntaxNode
        {
            return (TNode)node.SetDiagnostics(ImmutableArray<Diagnostic>.Empty);
        }
    }
}