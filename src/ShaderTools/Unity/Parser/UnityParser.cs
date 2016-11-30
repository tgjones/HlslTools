using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using ShaderTools.Core.Text;
using ShaderTools.Unity.Syntax;
using ShaderTools.Unity.Diagnostics;

namespace ShaderTools.Unity.Parser
{
    internal partial class UnityParser
    {
        private readonly UnityLexer _lexer;
        private readonly List<SyntaxToken> _tokens = new List<SyntaxToken>();
        private int _tokenIndex;

        private CancellationToken _cancellationToken;

        public UnityParser(UnityLexer lexer)
        {
            _lexer = lexer;
        }

        protected SyntaxToken Current => Peek(0);
        protected SyntaxToken Lookahead => Peek(1);

        private int EnsureToken(int tokenIndex)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            if (_tokens.Any() && _tokens.Last().Kind == SyntaxKind.EndOfFileToken && tokenIndex == _tokens.Count - 1)
                return _tokens.Count - 1;

            List<SyntaxToken> badTokens = null;

            while (tokenIndex >= _tokens.Count)
            {
                var token = _lexer.Lex();

                // Skip any bad tokens.
                if (token.Kind == SyntaxKind.BadToken)
                {
                    if (badTokens == null)
                        badTokens = new List<SyntaxToken>();
                    badTokens.Clear();
                    while (token.Kind == SyntaxKind.BadToken)
                    {
                        badTokens.Add(token);
                        token = _lexer.Lex();
                    }
                }

                if (badTokens != null && badTokens.Count > 0)
                {
                    var trivia = ImmutableArray.Create(CreateSkippedTokensTrivia(badTokens))
                                 .Concat(token.LeadingTrivia).ToImmutableArray();
                    token = token.WithLeadingTrivia(trivia);
                }

                _tokens.Add(token);

                if (token.Kind == SyntaxKind.EndOfFileToken)
                    break;
            }

            return Math.Min(tokenIndex, _tokens.Count - 1);
        }

        private SyntaxToken Peek(int offset)
        {
            var i = EnsureToken(_tokenIndex + offset);
            return _tokens[i];
        }

        protected SyntaxToken NextToken()
        {
            var result = Current;
            _tokenIndex++;
            return result;
        }

        private SyntaxToken NextTokenWithPrejudice(SyntaxKind kind)
        {
            var result = Current;
            if (result.Kind != kind)
            {
                var diagnostics = new List<Diagnostic>(result.Diagnostics);
                diagnostics.ReportTokenExpected(result.Span, result, kind);
                result = result.WithDiagnostics(diagnostics);
            }
            NextToken();
            return result;
        }

        protected SyntaxToken NextTokenIf(SyntaxKind kind)
        {
            return Current.Kind == kind
                ? NextToken()
                : null;
        }

        protected SyntaxToken Match(SyntaxKind kind)
        {
            if (Current.Kind == kind)
                return NextToken();

            if (Lookahead.Kind == kind)
            {
                // Skip current token, and carry on parsing.
                var skippedTokensTrivia = CreateSkippedTokensTrivia(new[] { Current });

                var diagnostics = new List<Diagnostic>(Lookahead.Diagnostics);
                diagnostics.ReportTokenUnexpected(Current.Span, Current);

                NextToken();

                var result = Current.WithDiagnostics(diagnostics).WithLeadingTrivia(new[] { skippedTokensTrivia });

                NextToken();

                return result;
            }

            // TODO: NQuery checks if user is currently typing an identifier, and
            // the partial identifier is a keyword.

            return InsertMissingToken(kind);
        }

        protected SyntaxToken MatchOneOf(SyntaxKind preferred, params SyntaxKind[] otherOptions)
        {
            var allOptions = new[] { preferred }.Concat(otherOptions).ToList();

            if (allOptions.Contains(Current.Kind))
                return NextToken();

            if (allOptions.Contains(Lookahead.Kind))
            {
                // Skip current token, and carry on parsing.
                var skippedTokensTrivia = CreateSkippedTokensTrivia(new[] { Current });

                var diagnostics = new List<Diagnostic>(Lookahead.Diagnostics);
                diagnostics.ReportTokenUnexpected(Current.Span, Current);

                NextToken();

                var result = Current.WithDiagnostics(diagnostics).WithLeadingTrivia(new[] { skippedTokensTrivia });

                NextToken();

                return result;
            }

            // TODO: NQuery checks if user is currently typing an identifier, and
            // the partial identifier is a keyword.

            return InsertMissingToken(preferred, otherOptions);
        }

        protected TNode WithDiagnostic<TNode>(TNode node, DiagnosticId diagnosticId, params object[] args)
            where TNode : SyntaxNode
        {
            var diagnostic = Diagnostic.Format(node.Span, diagnosticId, args);
            return node.WithDiagnostic(diagnostic);
        }

        protected SyntaxToken InsertMissingToken(SyntaxKind kind)
        {
            var missingTokenSourceRange = new TextSpan(_lexer.Text, Current.FullSpan.Start, 0);

            var diagnosticSpan = GetDiagnosticTextSpanForMissingToken();
            var diagnostics = new List<Diagnostic>(1);
            diagnostics.ReportTokenExpected(diagnosticSpan, Current, kind);

            return new SyntaxToken(kind, true, missingTokenSourceRange).WithDiagnostics(diagnostics);
        }

        protected SyntaxToken InsertMissingToken(SyntaxKind preferred, SyntaxKind[] otherOptions)
        {
            var missingTokenSourceRange = new TextSpan(_lexer.Text, Current.FullSpan.Start, 0);

            var diagnosticSpan = GetDiagnosticTextSpanForMissingToken();
            var diagnostics = new List<Diagnostic>(1);
            diagnostics.ReportTokenExpectedMultipleChoices(diagnosticSpan, Current, new[] { preferred }.Concat(otherOptions));

            return new SyntaxToken(preferred, true, missingTokenSourceRange).WithDiagnostics(diagnostics);
        }

        protected TextSpan GetDiagnosticTextSpanForMissingToken()
        {
            if (_tokenIndex > 0)
            {
                var previousToken = _tokens[_tokenIndex - 1];
                if (previousToken.TrailingTrivia.Any(x => x.Kind == SyntaxKind.EndOfLineTrivia))
                    return new TextSpan(previousToken.Span.SourceText, previousToken.Span.End, 2);
            }

            return Current.Span;
        }

        private StructuredTriviaSyntax CreateSkippedTokensTrivia(IReadOnlyCollection<SyntaxToken> tokens)
        {
            return new SkippedTokensTriviaSyntax(tokens);
        }
    }
}