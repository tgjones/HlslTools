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
                firstToken.Span.SourceText,
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
                firstToken.Span.SourceText,
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
                firstToken.Span.SourceText,
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

        public static IEnumerable<SyntaxNode> FindNodes(this SyntaxNode root, SourceLocation position)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            // NOTE: We don't use Distinct() because we want to preserve the order of nodes.
            var seenNodes = new HashSet<SyntaxNode>();
            return root.FindStartTokens(position)
                .SelectMany(t => t.Parent.AncestorsAndSelf())
                .Where(seenNodes.Add);
        }

        public static IEnumerable<SyntaxNode> DescendantNodes(this SyntaxNode root)
        {
            return root.DescendantNodesAndSelf().Skip(1);
        }

        public static IEnumerable<SyntaxNode> DescendantNodesAndSelf(this SyntaxNode root)
        {
            var stack = new Stack<SyntaxNode>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current;

                foreach (var child in current.ChildNodes.Reverse())
                    stack.Push(child);
            }
        }

        public static bool PossiblyInUserGivenName(this SyntaxTree tree, SourceLocation position)
        {
            if (tree == null)
                throw new ArgumentNullException(nameof(tree));

            var token = tree.Root.FindTokenOnLeft(position);

            return PossiblyInTypeDefinitionName(token, position)
                || PossiblyInConstantBufferDefinitionName(token, position)
                || PossiblyInFunctionDeclarationName(token, position)
                || PossiblyInVariableDeclarationName(token, position);
        }

        private static bool PossiblyInTypeDefinitionName(SyntaxToken token, SourceLocation position)
        {
            var node = token.Parent as TypeDefinitionSyntax;
            return node != null && node.NameToken.SourceRange.ContainsOrTouches(position);
        }

        private static bool PossiblyInConstantBufferDefinitionName(SyntaxToken token, SourceLocation position)
        {
            var node = token.Parent as ConstantBufferSyntax;
            return node != null && node.Name.SourceRange.ContainsOrTouches(position);
        }

        private static bool PossiblyInFunctionDeclarationName(SyntaxToken token, SourceLocation position)
        {
            var node = token.Parent as FunctionSyntax;
            return node != null && node.Name.SourceRange.ContainsOrTouches(position);
        }

        private static bool PossiblyInVariableDeclarationName(SyntaxToken token, SourceLocation position)
        {
            var node = token.Parent as VariableDeclaratorSyntax;
            return node != null && node.Identifier.SourceRange.ContainsOrTouches(position);
        }

        public static bool PossiblyInTypeName(this SyntaxTree tree, SourceLocation position)
        {
            if (tree == null)
                throw new ArgumentNullException(nameof(tree));

            var token = tree.Root.FindTokenOnLeft(position);
            var parent = GetNonIdentifierParent(token);

            if (parent.Kind == SyntaxKind.SkippedTokensTrivia)
                return true;

            return PossiblyInFunctionReturnTypeName(parent, position)
                || PossiblyInParameterTypeName(parent, position)
                || PossiblyInVariableTypeName(parent, position);
        }

        private static bool PossiblyInFunctionReturnTypeName(SyntaxNode tokenParent, SourceLocation position)
        {
            var node = tokenParent as FunctionSyntax;
            return node != null && node.ReturnType.SourceRange.ContainsOrTouches(position);
        }

        private static bool PossiblyInParameterTypeName(SyntaxNode tokenParent, SourceLocation position)
        {
            var node = tokenParent as ParameterSyntax;
            return node != null && node.Type.SourceRange.ContainsOrTouches(position);
        }

        private static bool PossiblyInVariableTypeName(SyntaxNode tokenParent, SourceLocation position)
        {
            var node = tokenParent as VariableDeclarationSyntax;
            return node != null && node.Type.SourceRange.ContainsOrTouches(position);
        }

        private static SyntaxNode GetNonIdentifierParent(SyntaxToken token)
        {
            var node = token.Parent as IdentifierNameSyntax;
            if (node != null)
                return node.Parent;
            return token.Parent;
        }
    }
}