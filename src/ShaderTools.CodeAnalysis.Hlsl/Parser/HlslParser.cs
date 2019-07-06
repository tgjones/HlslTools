using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Parser
{
    internal partial class HlslParser
    {
        private readonly ILexer _lexer;
        private readonly List<SyntaxToken> _tokens = new List<SyntaxToken>();
        private int _tokenIndex;
        private LexerMode _mode;

        private CancellationToken _cancellationToken;

        // Set to true when parsing generic template arguments.
        private bool _greaterThanTokenIsNotOperator;
        private bool _allowLinearAndPointAsIdentifiers;

        // Set to true when parsing pass statements.
        private bool _allowGreaterThanTokenAroundRhsExpression;

        // Used when parsing function calls, initializers, and variable declarations.
        protected Stack<bool> CommaIsSeparatorStack { get; private set; }

        public HlslParser(ILexer lexer, LexerMode mode = LexerMode.Syntax)
        {
            _lexer = lexer;
            _mode = mode;

            CommaIsSeparatorStack = new Stack<bool>();
            CommaIsSeparatorStack.Push(false);
        }

        protected SyntaxToken Current => Peek(0);
        protected SyntaxToken Lookahead => Peek(1);

        private int EnsureToken(int tokenIndex)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            if (_tokens.Count > 0 && _tokens.Last().Kind == SyntaxKind.EndOfFileToken && tokenIndex == _tokens.Count - 1)
                return _tokens.Count - 1;

            List<SyntaxToken> badTokens = null;

            while (tokenIndex >= _tokens.Count)
            {
                var token = _lexer.Lex(_mode);

                // Skip any bad tokens.
                if (token.Kind == SyntaxKind.BadToken)
                {
                    if (badTokens == null)
                        badTokens = new List<SyntaxToken>();
                    badTokens.Clear();
                    while (token.Kind == SyntaxKind.BadToken)
                    {
                        badTokens.Add(token);
                        token = _lexer.Lex(_mode);
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
                diagnostics.ReportTokenExpected(result.SourceRange, result, kind);
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
                diagnostics.ReportTokenUnexpected(Current.SourceRange, Current);

                NextToken();

                var result = Current.WithDiagnostics(diagnostics).WithLeadingTrivia(new[] { skippedTokensTrivia });

                NextToken();

                return result;
            }

            // TODO: NQuery checks if user is currently typing an identifier, and
            // the partial identifier is a keyword.

            return InsertMissingToken(kind);
        }

        protected TNode WithDiagnostic<TNode>(TNode node, DiagnosticId diagnosticId, params object[] args)
            where TNode : SyntaxNode
        {
            var diagnostic = Diagnostic.Create(HlslMessageProvider.Instance, node.SourceRange, (int) diagnosticId, args);
            return node.WithDiagnostic(diagnostic);
        }

        protected SyntaxToken InsertMissingToken(SyntaxKind kind)
        {
            var missingTokenSourceRange = new SourceRange(Current.FullSourceRange.Start, 0);

            var missingTokenSpan = new SourceFileSpan(Current.FileSpan.File, new TextSpan(Current.FileSpan.Span.Start, 0));
            var leadingLocatedTrivia = Current.LeadingTrivia.OfType<LocatedNode>().FirstOrDefault();
            if (leadingLocatedTrivia != null)
                missingTokenSpan = new SourceFileSpan(leadingLocatedTrivia.FileSpan.File, new TextSpan(leadingLocatedTrivia.FileSpan.Span.Start, 0));
            
            var diagnosticSpan = GetDiagnosticSourceRangeForMissingToken();
            var diagnostics = new List<Diagnostic>(1);
            diagnostics.ReportTokenExpected(diagnosticSpan, Current, kind);

            return new SyntaxToken(kind, true, missingTokenSourceRange, missingTokenSpan).WithDiagnostics(diagnostics);
        }

        protected SourceRange GetDiagnosticSourceRangeForMissingToken()
        {
            if (_tokenIndex > 0)
            {
                var previousToken = _tokens[_tokenIndex - 1];
                if (previousToken.TrailingTrivia.Any(x => x.Kind == SyntaxKind.EndOfLineTrivia))
                    return new SourceRange(previousToken.SourceRange.End, 2);
            }

            return Current.SourceRange;
        }

        protected SourceFileSpan GetDiagnosticTextSpanForMissingToken()
        {
            if (_tokenIndex > 0)
            {
                var previousToken = _tokens[_tokenIndex - 1];
                if (previousToken.TrailingTrivia.Any(x => x.Kind == SyntaxKind.EndOfLineTrivia))
                    return new SourceFileSpan(previousToken.FileSpan.File, new TextSpan(previousToken.FileSpan.Span.End, 2));
            }

            return Current.FileSpan;
        }

        private StructuredTriviaSyntax CreateSkippedTokensTrivia(IReadOnlyCollection<SyntaxToken> tokens)
        {
            return new SkippedTokensTriviaSyntax(tokens);
        }

        private struct ResetPoint
        {
            public readonly int TokenIndex;
            public readonly LexerMode Mode;
            public readonly bool GreaterThanTokenIsNotOperator;
            public readonly bool AllowLinearAndPointAsIdentifiers;
            public readonly TerminatorState TermState;
            public readonly Stack<bool> CommaIsSeparatorStack;

            internal ResetPoint(
                int tokenIndex, LexerMode mode,
                bool greaterThanTokenIsNotOperator, bool allowLinearAndPointAsIdentifiers,
                Stack<bool> commaIsSeparatorStack, TerminatorState termState)
            {
                TokenIndex = tokenIndex;
                Mode = mode;
                GreaterThanTokenIsNotOperator = greaterThanTokenIsNotOperator;
                AllowLinearAndPointAsIdentifiers = allowLinearAndPointAsIdentifiers;
                CommaIsSeparatorStack = new Stack<bool>(commaIsSeparatorStack.Reverse());
                TermState = termState;
            }
        }

        private readonly Stack<bool> _scanStack = new Stack<bool>();

        private ResetPoint GetResetPoint()
        {
            _scanStack.Push(true);
            return new ResetPoint(_tokenIndex, _mode, _greaterThanTokenIsNotOperator, _allowLinearAndPointAsIdentifiers, CommaIsSeparatorStack, _termState);
        }

        private void Reset(ref ResetPoint state)
        {
            _scanStack.Pop();

            _mode = state.Mode;
            _tokenIndex = state.TokenIndex;
            _greaterThanTokenIsNotOperator = state.GreaterThanTokenIsNotOperator;
            _allowLinearAndPointAsIdentifiers = state.AllowLinearAndPointAsIdentifiers;
            CommaIsSeparatorStack = state.CommaIsSeparatorStack;
            _termState = state.TermState;
        }

        protected IdentifierNameSyntax CreateMissingIdentifierName()
        {
            return new IdentifierNameSyntax(InsertMissingToken(SyntaxKind.IdentifierToken));
        }

        private AnnotationsSyntax ParseAnnotations()
        {
            var lessThan = Match(SyntaxKind.LessThanToken);

            var annotations = new List<VariableDeclarationStatementSyntax>();
            while (Current.Kind != SyntaxKind.GreaterThanToken)
            {
                if (IsPossibleVariableDeclarationStatement())
                    annotations.Add(ParseVariableDeclarationStatement());
                else
                {
                    var action = SkipBadTokens(
                        p => p.Current.Kind != SyntaxKind.GreaterThanToken,
                        p => p.IsTerminator());
                    if (action == PostSkipAction.Abort)
                        break;
                }
            }

            var greaterThan = Match(SyntaxKind.GreaterThanToken);

            return new AnnotationsSyntax(lessThan, annotations, greaterThan);
        }

        private bool IsPossibleAttribute()
        {
            return Current.Kind == SyntaxKind.OpenBracketToken;
        }

        private List<AttributeSyntax> ParseAttributes()
        {
            var attributes = new List<AttributeSyntax>();
            while (IsPossibleAttribute())
                attributes.Add(ParseAttribute());
            return attributes;
        }

        private AttributeSyntax ParseAttribute()
        {
            var openBracket = Match(SyntaxKind.OpenBracketToken);
            var name = ParseIdentifier();
            var argumentList = ParseAttributeArgumentList();
            var closeBracket = Match(SyntaxKind.CloseBracketToken);

            return new AttributeSyntax(openBracket, name, argumentList, closeBracket);
        }

        private AttributeArgumentListSyntax ParseAttributeArgumentList()
        {
            AttributeArgumentListSyntax result = null;

            if (Current.Kind == SyntaxKind.OpenParenToken)
            {
                CommaIsSeparatorStack.Push(true);

                try
                {
                    var openParen = Match(SyntaxKind.OpenParenToken);

                    var argumentsList = new List<SyntaxNodeBase>();
                    argumentsList.Add(ParseExpression());

                    while (Current.Kind == SyntaxKind.CommaToken)
                    {
                        argumentsList.Add(Match(SyntaxKind.CommaToken));
                        argumentsList.Add(ParseExpression());
                    }

                    var closeParen = Match(SyntaxKind.CloseParenToken);

                    result = new AttributeArgumentListSyntax(
                        openParen,
                        new SeparatedSyntaxList<LiteralExpressionSyntax>(argumentsList),
                        closeParen);
                }
                finally
                {
                    CommaIsSeparatorStack.Pop();
                }
            }

            return result;
        }

        private void ParseTopLevelDeclarations(List<SyntaxNode> declarations, SyntaxKind endKind)
        {
            while (Current.Kind != endKind)
            {
                switch (Current.Kind)
                {
                    case SyntaxKind.NamespaceKeyword:
                        declarations.Add(ParseNamespace());
                        break;
                    case SyntaxKind.CBufferKeyword:
                    case SyntaxKind.TBufferKeyword:
                        declarations.Add(ParseConstantBuffer());
                        break;
                    case SyntaxKind.TechniqueKeyword:
                    case SyntaxKind.Technique10Keyword:
                    case SyntaxKind.Technique11Keyword:
                        declarations.Add(ParseTechnique());
                        break;
                    case SyntaxKind.SemiToken:
                        declarations.Add(new EmptyStatementSyntax(new List<AttributeSyntax>(), NextToken()));
                        break;
                    default:
                        if (IsPossibleFunctionDeclaration())
                            declarations.Add(ParseFunctionDefinitionOrDeclaration(false));
                        else if (IsPossibleDeclarationStatement())
                            declarations.Add(ParseDeclarationStatement());
                        else
                        {
                            var result = SkipBadTokens(
                                p => !p.IsPossibleGlobalDeclarationStart(),
                                p => p.Current.Kind == SyntaxKind.EndOfFileToken);
                            if (result == PostSkipAction.Abort)
                                return;
                        }
                        break;
                }
            }
        }

        private NamespaceSyntax ParseNamespace()
        {
            var @namespace = NextToken();
            var name = Match(SyntaxKind.IdentifierToken);

            var openBrace = Match(SyntaxKind.OpenBraceToken);

            var declarations = new List<SyntaxNode>();
            ParseTopLevelDeclarations(declarations, SyntaxKind.CloseBraceToken);

            var closeBrace = Match(SyntaxKind.CloseBraceToken);
            var semicolon = NextTokenIf(SyntaxKind.SemiToken);

            return new NamespaceSyntax(@namespace, name, openBrace, declarations, closeBrace, semicolon);
        }

        public CompilationUnitSyntax ParseCompilationUnit(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;

            var saveTerm = _termState;
            _termState |= TerminatorState.IsPossibleGlobalDeclarationStartOrStop;

            var declarations = new List<SyntaxNode>();
            ParseTopLevelDeclarations(declarations, SyntaxKind.EndOfFileToken);

            _termState = saveTerm;

            var eof = Match(SyntaxKind.EndOfFileToken);

            return new CompilationUnitSyntax(declarations, eof);
        }
    }
}