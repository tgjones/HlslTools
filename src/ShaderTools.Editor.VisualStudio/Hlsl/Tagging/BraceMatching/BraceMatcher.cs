using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Tagging.BraceMatching
{
    [Export]
    internal sealed class BraceMatcher
    {
        private readonly List<Tuple<SyntaxKind, SyntaxKind>> _matchingKinds;

        public BraceMatcher()
        {
            _matchingKinds = new List<Tuple<SyntaxKind, SyntaxKind>>
            {
                Tuple.Create(SyntaxKind.OpenBraceToken, SyntaxKind.CloseBraceToken),
                Tuple.Create(SyntaxKind.OpenBracketToken, SyntaxKind.CloseBracketToken),
                Tuple.Create(SyntaxKind.OpenParenToken, SyntaxKind.CloseParenToken)
            };
        }

        public BraceMatchingResult MatchBraces(SyntaxTree syntaxTree, SourceLocation position)
        {
            return _matchingKinds
                .Select(k => MatchBraces(syntaxTree, position, k.Item1, k.Item2))
                .Where(r => r.IsValid)
                .DefaultIfEmpty(BraceMatchingResult.None)
                .First();
        }

        private static BraceMatchingResult MatchBraces(SyntaxTree syntaxTree, SourceLocation position, SyntaxKind leftKind, SyntaxKind rightKind)
        {
            return syntaxTree.Root.FindStartTokens(position)
                .Select(t => MatchBraces(t, position, leftKind, rightKind))
                .Where(r => r.IsValid)
                .DefaultIfEmpty(BraceMatchingResult.None)
                .First();
        }

        private static BraceMatchingResult MatchBraces(SyntaxToken token, SourceLocation position, SyntaxKind leftKind, SyntaxKind rightKind)
        {
            var isLeft = token.Kind == leftKind &&
                         position == token.SourceRange.Start;

            var isRight = token.Kind == rightKind &&
                          position == token.SourceRange.End;

            if (isLeft)
            {
                var left = token.Span;
                SourceFileSpan right;
                if (FindMatchingBrace(position, 1, token.Parent, rightKind, out right))
                    return MapResultToFile(left, right);
            }
            else if (isRight)
            {
                SourceFileSpan left;
                var right = token.Span;
                if (FindMatchingBrace(position, -1, token.Parent, leftKind, out left))
                    return MapResultToFile(left, right);
            }

            return BraceMatchingResult.None;
        }

        private static BraceMatchingResult MapResultToFile(SourceFileSpan left, SourceFileSpan right)
        {
            if (!left.File.IsRootFile || !right.File.IsRootFile)
                return BraceMatchingResult.None;

            return new BraceMatchingResult(left.Span, right.Span);
        }

        private static bool FindMatchingBrace(SourceLocation position, int direction, SyntaxNode parent, SyntaxKind syntaxKind, out SourceFileSpan right)
        {
            var tokens = parent.ChildNodes.Where(t => t.IsKind(syntaxKind));
            var relevantTokens = (direction < 0)
                ? from t in tokens
                    where t.SourceRange.End <= position
                    select t
                : from t in tokens
                    where position < t.SourceRange.Start
                    select t;

            right = default(SourceFileSpan);
            var found = false;

            foreach (var token in relevantTokens.Cast<SyntaxToken>())
            {
                if (!found)
                {
                    right = token.Span;
                    found = true;
                }
                else
                    return false;
            }

            return found;
        }
    }
}