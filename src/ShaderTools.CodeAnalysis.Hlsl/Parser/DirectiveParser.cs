using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Hlsl.Parser
{
    internal sealed class DirectiveParser : HlslParser
    {
        private readonly HlslLexer _lexer;
        private readonly DirectiveStack _directiveStack;

        public DirectiveParser(HlslLexer lexer, DirectiveStack directiveStack)
            : base(lexer, LexerMode.Directive)
        {
            _lexer = lexer;
            _directiveStack = directiveStack;
        }

        public SyntaxNode ParseDirective(bool isActive, bool endIsActive, bool isAfterNonWhitespaceOnLine)
        {
            var hash = Match(SyntaxKind.HashToken);

            if (isAfterNonWhitespaceOnLine)
                hash = WithDiagnostic(hash, DiagnosticId.BadDirectivePlacement);

            switch (Current.ContextualKind)
            {
                case SyntaxKind.IfKeyword:
                    return ParseIfDirective(hash, MatchContextual(SyntaxKind.IfKeyword), isActive);
                case SyntaxKind.IfDefKeyword:
                    return ParseIfDefDirective(hash, MatchContextual(SyntaxKind.IfDefKeyword), isActive);
                case SyntaxKind.IfNDefKeyword:
                    return ParseIfNDefDirective(hash, MatchContextual(SyntaxKind.IfNDefKeyword), isActive);
                case SyntaxKind.ElseKeyword:
                    return ParseElseDirective(hash, MatchContextual(SyntaxKind.ElseKeyword), isActive, endIsActive);
                case SyntaxKind.ElifKeyword:
                    return ParseElifDirective(hash, MatchContextual(SyntaxKind.ElifKeyword), isActive, endIsActive);
                case SyntaxKind.EndIfKeyword:
                    return ParseEndIfDirective(hash, MatchContextual(SyntaxKind.EndIfKeyword), isActive, endIsActive);
                case SyntaxKind.DefineKeyword:
                    return ParseDefineDirective(hash, MatchContextual(SyntaxKind.DefineKeyword), isActive);
                case SyntaxKind.UndefKeyword:
                    return ParseUndefDirective(hash, MatchContextual(SyntaxKind.UndefKeyword), isActive);
                case SyntaxKind.IncludeKeyword:
                    return ParseIncludeDirective(hash, MatchContextual(SyntaxKind.IncludeKeyword), isActive);
                case SyntaxKind.LineKeyword:
                    return ParseLineDirective(hash, MatchContextual(SyntaxKind.LineKeyword), isActive);
                case SyntaxKind.ErrorKeyword:
                    return ParseErrorDirective(hash, MatchContextual(SyntaxKind.ErrorKeyword), isActive);
                case SyntaxKind.PragmaKeyword:
                    return ParsePragmaDirective(hash, MatchContextual(SyntaxKind.PragmaKeyword), isActive);
                default:
                    var id = Match(SyntaxKind.IdentifierToken);
                    var end = ParseEndOfDirective(ignoreErrors: true);
                    if (!isAfterNonWhitespaceOnLine)
                    {
                        if (!id.IsMissing)
                            id = WithDiagnostic(id, DiagnosticId.DirectiveExpected);
                        else
                            hash = WithDiagnostic(hash, DiagnosticId.DirectiveExpected);
                    }

                    return new BadDirectiveTriviaSyntax(hash, id, end, isActive);
            }
        }

        private IfDirectiveTriviaSyntax ParseIfDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive)
        {
            _lexer.ExpandMacros = true;
            var expr = ParseDirectiveExpression();
            _lexer.ExpandMacros = false;

            var eod = ParseEndOfDirective(false);

            var isTrue = EvaluateBool(expr);
            var branchTaken = isActive && isTrue;

            return new IfDirectiveTriviaSyntax(
                hash, keyword, expr, eod,
                isActive, branchTaken, isTrue);
        }

        private IfDefDirectiveTriviaSyntax ParseIfDefDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive)
        {
            var name = Match(SyntaxKind.IdentifierToken);
            var eod = ParseEndOfDirective(false);

            var isTrue = _directiveStack.IsDefined(name.Text) == DefineState.Defined;
            var branchTaken = isActive && isTrue;

            return new IfDefDirectiveTriviaSyntax(
                hash, keyword, name, eod,
                isActive, branchTaken, isTrue);
        }

        private IfNDefDirectiveTriviaSyntax ParseIfNDefDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive)
        {
            var name = Match(SyntaxKind.IdentifierToken);
            var eod = ParseEndOfDirective(false);

            var isTrue = _directiveStack.IsDefined(name.Text) != DefineState.Defined;
            var branchTaken = isActive && isTrue;

            return new IfNDefDirectiveTriviaSyntax(
                hash, keyword, name, eod,
                isActive, branchTaken, isTrue);
        }

        private DirectiveTriviaSyntax ParseElseDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive, bool endIsActive)
        {
            var eod = ParseEndOfDirective(false);

            if (_directiveStack.HasPreviousIfOrElif())
            {
                var branchTaken = endIsActive && !_directiveStack.PreviousBranchTaken();
                return new ElseDirectiveTriviaSyntax(
                    hash,
                    keyword,
                    eod,
                    endIsActive, branchTaken);
            }

            if (_directiveStack.HasUnfinishedIf())
                return WithDiagnostic(new BadDirectiveTriviaSyntax(hash, keyword, eod, isActive), DiagnosticId.EndIfDirectiveExpected, $"Expected #endif directive");

            return WithDiagnostic(new BadDirectiveTriviaSyntax(hash, keyword, eod, isActive), DiagnosticId.UnexpectedDirective, $"Unexpected directive: '{keyword}'");
        }

        private DirectiveTriviaSyntax ParseElifDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive, bool endIsActive)
        {
            _lexer.ExpandMacros = true;
            var expr = ParseDirectiveExpression();
            _lexer.ExpandMacros = false;

            var eod = ParseEndOfDirective(false);

            if (_directiveStack.HasPreviousIfOrElif())
            {
                var isTrue = EvaluateBool(expr);
                var branchTaken = endIsActive && isTrue && !_directiveStack.PreviousBranchTaken();
                return new ElifDirectiveTriviaSyntax(
                    hash, keyword, expr, eod,
                    endIsActive, branchTaken, isTrue);
            }

            var exprFirstToken = expr.GetFirstToken();
            var exprLastToken = expr.GetLastToken();

            eod = eod.WithLeadingTrivia(eod.LeadingTrivia.Add(new SyntaxTrivia(SyntaxKind.DisabledTextTrivia, expr.ToFullString(),
                expr.SourceRange, _lexer.CreateSourceFileSpan(TextSpan.FromBounds(exprFirstToken.FileSpan.Span.Start, exprLastToken.FileSpan.Span.End)),
                ImmutableArray<Diagnostic>.Empty)).ToArray());
            if (_directiveStack.HasUnfinishedIf())
                return WithDiagnostic(new BadDirectiveTriviaSyntax(hash, keyword, eod, isActive), DiagnosticId.EndIfDirectiveExpected);

            return WithDiagnostic(new BadDirectiveTriviaSyntax(hash, keyword, eod, isActive), DiagnosticId.UnexpectedDirective, keyword);
        }

        private DirectiveTriviaSyntax ParseEndIfDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive, bool endIsActive)
        {
            var eod = ParseEndOfDirective(false);

            if (_directiveStack.HasUnfinishedIf())
                return new EndIfDirectiveTriviaSyntax(hash, keyword, eod, endIsActive);

            return WithDiagnostic(new BadDirectiveTriviaSyntax(hash, keyword, eod, isActive), DiagnosticId.UnexpectedDirective, $"Unexpected directive: '{keyword}'");
        }

        private DefineDirectiveTriviaSyntax ParseDefineDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive)
        {
            var name = (Current.Kind.IsIdentifierOrKeyword())
                ? NextToken()
                : Match(SyntaxKind.IdentifierToken);

            // Left paren must immediately follow name, with no whitespace.
            if (Current.Kind == SyntaxKind.OpenParenToken && Current.SourceRange.Start == name.SourceRange.End)
                return ParseFunctionLikeDefineDirective(hash, keyword, name, isActive);

            return ParseObjectLikeDefineDirective(hash, keyword, name, isActive);
        }

        private UndefDirectiveTriviaSyntax ParseUndefDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive)
        {
            var name = (Current.Kind.IsIdentifierOrKeyword())
                ? NextToken()
                : Match(SyntaxKind.IdentifierToken);
            var eod = ParseEndOfDirective(false);

            return new UndefDirectiveTriviaSyntax(hash, keyword, name, eod, isActive);
        }

        private FunctionLikeDefineDirectiveTriviaSyntax ParseFunctionLikeDefineDirective(SyntaxToken hash, SyntaxToken keyword, SyntaxToken name, bool isActive)
        {
            var openParen = Match(SyntaxKind.OpenParenToken);

            var paramList = new List<SyntaxNodeBase>();
            if (Current.Kind != SyntaxKind.CloseParenToken)
            {
                if (Current.Kind.IsKeyword())
                {
                    paramList.Add(NextToken().WithKind(SyntaxKind.IdentifierToken));
                }
                else
                {
                    paramList.Add(Match(SyntaxKind.IdentifierToken));
                }

                while (Current.Kind == SyntaxKind.CommaToken)
                {
                    paramList.Add(Match(SyntaxKind.CommaToken));

                    switch (Current.Kind)
                    {
                        case SyntaxKind.IdentifierToken:
                            paramList.Add(Match(SyntaxKind.IdentifierToken));
                            break;

                        default:
                            if (Current.Kind.IsKeyword())
                            {
                                paramList.Add(NextToken().WithKind(SyntaxKind.IdentifierToken));
                                break;
                            }
                            goto case SyntaxKind.IdentifierToken;
                    }
                }
            }

            var closeParen = Match(SyntaxKind.CloseParenToken);

            var parameters = new FunctionLikeDefineDirectiveParameterListSyntax(openParen, new SeparatedSyntaxList<SyntaxToken>(paramList), closeParen);

            var body = new List<SyntaxToken>();
            while (Current.Kind != SyntaxKind.EndOfDirectiveToken)
                body.Add(NextToken());

            var eod = ParseEndOfDirective(name.IsMissing);

            return new FunctionLikeDefineDirectiveTriviaSyntax(hash, keyword, name, parameters, body, eod, isActive);
        }

        private DefineDirectiveTriviaSyntax ParseObjectLikeDefineDirective(SyntaxToken hash, SyntaxToken keyword, SyntaxToken name, bool isActive)
        {
            var body = new List<SyntaxToken>();
            while (Current.Kind != SyntaxKind.EndOfDirectiveToken)
                body.Add(NextToken());

            var eod = ParseEndOfDirective(name.IsMissing);

            return new ObjectLikeDefineDirectiveTriviaSyntax(hash, keyword, name, body, eod, isActive);
        }

        private IncludeDirectiveTriviaSyntax ParseIncludeDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive)
        {
            var filename = Match((Current.Kind == SyntaxKind.BracketedStringLiteralToken)
                ? SyntaxKind.BracketedStringLiteralToken
                : SyntaxKind.StringLiteralToken);
            var eod = ParseEndOfDirective(filename.IsMissing);

            return new IncludeDirectiveTriviaSyntax(hash, keyword, filename, eod, isActive);
        }

        private LineDirectiveTriviaSyntax ParseLineDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive)
        {
            var line = Match(SyntaxKind.IntegerLiteralToken);
            var filename = Match(SyntaxKind.StringLiteralToken);
            var eod = ParseEndOfDirective(line.IsMissing || !isActive);

            return new LineDirectiveTriviaSyntax(hash, keyword, line, filename, eod, isActive);
        }

        private ErrorDirectiveTriviaSyntax ParseErrorDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive)
        {
            var body = new List<SyntaxToken>();
            while (Current.Kind != SyntaxKind.EndOfDirectiveToken)
                body.Add(NextToken());

            var eod = ParseEndOfDirective(false);

            return new ErrorDirectiveTriviaSyntax(hash, keyword, body, eod, isActive);
        }

        private PragmaDirectiveTriviaSyntax ParsePragmaDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive)
        {
            var body = new List<SyntaxToken>();
            while (Current.Kind != SyntaxKind.EndOfDirectiveToken)
                body.Add(NextToken());

            var eod = ParseEndOfDirective(false);

            return new PragmaDirectiveTriviaSyntax(hash, keyword, body, eod, isActive);
        }

        private ExpressionSyntax ParseDirectiveExpression()
        {
            return ParseDirectiveSubExpression(0);
        }

        private ExpressionSyntax ParseDirectiveSubExpression(uint precedence)
        {
            ExpressionSyntax leftOperand;
            SyntaxKind opKind;

            // No left operand, so we need to parse one -- possibly preceded by a
            // unary operator.
            var tk = Current.Kind;
            if (SyntaxFacts.IsPrefixUnaryExpression(tk, true))
            {
                opKind = SyntaxFacts.GetPrefixUnaryExpression(tk);
                leftOperand = ParseDirectivePrefixUnaryExpression(opKind);
            }
            else
            {
                // Not a unary operator - get a primary expression.
                leftOperand = ParseDirectiveTerm();
            }

            while (true)
            {
                // We either have a binary operator here, or we're finished.
                tk = Current.Kind;

                if (SyntaxFacts.IsBinaryExpression(tk))
                    opKind = SyntaxFacts.GetBinaryExpression(tk);
                else
                    break;

                var newPrecedence = SyntaxFacts.GetOperatorPrecedence(opKind);

                Debug.Assert(newPrecedence > 0); // All binary operators must have precedence > 0!

                // Check the precedence to see if we should "take" this operator
                if (newPrecedence < precedence)
                    break;

                // Same precedence, but not right-associative -- deal with this "later"
                if (newPrecedence == precedence && !SyntaxFacts.IsRightAssociative(opKind))
                    break;

                // Precedence is okay, so we'll "take" this operator.
                var opToken = NextToken();

                var rightOperand = ParseDirectiveSubExpression(newPrecedence);
                leftOperand = new BinaryExpressionSyntax(opKind, leftOperand, opToken, rightOperand);
            }

            var conditionalPrecedence = SyntaxFacts.GetOperatorPrecedence(SyntaxKind.ConditionalExpression);
            if (tk == SyntaxKind.QuestionToken && precedence <= conditionalPrecedence)
            {
                var questionToken = NextToken();

                var colonLeft = ParseDirectiveSubExpression(conditionalPrecedence);
                var colon = Match(SyntaxKind.ColonToken);

                var colonRight = ParseDirectiveSubExpression(conditionalPrecedence);
                leftOperand = new ConditionalExpressionSyntax(leftOperand, questionToken, colonLeft, colon, colonRight);
            }

            return leftOperand;
        }

        private ExpressionSyntax ParseDirectivePrefixUnaryExpression(SyntaxKind unaryExpression)
        {
            var operatorToken = NextToken();
            var operatorPrecedence = SyntaxFacts.GetOperatorPrecedence(unaryExpression);
            var operand = ParseDirectiveSubExpression(operatorPrecedence);
            return new PrefixUnaryExpressionSyntax(unaryExpression, operatorToken, operand);
        }

        private ExpressionSyntax ParseDirectiveTerm()
        {
            ExpressionSyntax expr;

            var tk = Current.Kind;
            switch (tk)
            {
                case SyntaxKind.IdentifierToken:
                    expr = ParseIdentifier();
                    break;
                case SyntaxKind.FalseKeyword:
                case SyntaxKind.TrueKeyword:
                case SyntaxKind.IntegerLiteralToken:
                case SyntaxKind.FloatLiteralToken:
                    expr = new LiteralExpressionSyntax(SyntaxFacts.GetLiteralExpression(Current.Kind), NextToken());
                    break;
                case SyntaxKind.OpenParenToken:
                    expr = ParseDirectiveParenthesizedExpression();
                    break;
                default:
                    expr = CreateMissingIdentifierName();

                    if (tk == SyntaxKind.EndOfFileToken)
                        expr = WithDiagnostic(expr, DiagnosticId.ExpressionExpected);
                    else
                        expr = WithDiagnostic(expr, DiagnosticId.InvalidExprTerm, tk.GetText());

                    break;
            }

            // Check for `defined` usage.
            if (expr.Kind == SyntaxKind.IdentifierName
                && ((IdentifierNameSyntax) expr).Name.ContextualKind == SyntaxKind.DefinedKeyword)
            {
                _lexer.ExpandMacros = false;

                var openParen = (Current.Kind == SyntaxKind.OpenParenToken)
                    ? Match(SyntaxKind.OpenParenToken)
                    : null;

                var name = new IdentifierNameSyntax(NextToken());

                _lexer.ExpandMacros = true;

                var closeParen = (openParen != null)
                    ? Match(SyntaxKind.CloseParenToken)
                    : null;

                expr = new FunctionInvocationExpressionSyntax((IdentifierNameSyntax) expr, new ArgumentListSyntax(
                    openParen,
                    new SeparatedSyntaxList<ExpressionSyntax>(new List<SyntaxNodeBase> { name }),
                    closeParen));
            }

            return expr;
        }

        private ExpressionSyntax ParseDirectiveParenthesizedExpression()
        {
            var openParen = Match(SyntaxKind.OpenParenToken);
            var expression = ParseDirectiveSubExpression(0);
            var closeParen = Match(SyntaxKind.CloseParenToken);
            return new ParenthesizedExpressionSyntax(openParen, expression, closeParen);
        }

        private SyntaxToken ParseEndOfDirective(bool ignoreErrors, bool afterLineNumber = false)
        {
            var skippedTokens = new List<SyntaxToken>();

            // Consume all extranous tokens as leading SkippedTokens trivia.
            if (Current.Kind != SyntaxKind.EndOfDirectiveToken &&
                Current.Kind != SyntaxKind.EndOfFileToken)
            {
                skippedTokens = new List<SyntaxToken>(10);

                if (!ignoreErrors)
                {
                    var errorCode = DiagnosticId.EndOfPreprocessorLineExpected;
                    if (afterLineNumber)
                        errorCode = DiagnosticId.MissingPreprocessorFile;

                    skippedTokens.Add(WithDiagnostic(NextToken().WithoutDiagnostics(), errorCode));
                }

                while (Current.Kind != SyntaxKind.EndOfDirectiveToken &&
                       Current.Kind != SyntaxKind.EndOfFileToken)
                {
                    skippedTokens.Add(NextToken().WithoutDiagnostics());
                }
            }

            // attach text from extraneous tokens as trivia to EndOfDirective token
            var endOfDirective = Current.Kind == SyntaxKind.EndOfDirectiveToken
                ? NextToken()
                : new SyntaxToken(SyntaxKind.EndOfDirectiveToken, true,
                    GetDiagnosticSourceRangeForMissingToken(),
                    GetDiagnosticTextSpanForMissingToken());

            if (skippedTokens.Any())
                endOfDirective = endOfDirective.WithLeadingTrivia(new[] { new SkippedTokensTriviaSyntax(skippedTokens.ToList()) });

            return endOfDirective;
        }

        private SyntaxToken MatchContextual(SyntaxKind kind)
        {
            if (Current.ContextualKind == kind)
            {
                var result = NextToken();
                result = result.WithKind(result.ContextualKind);
                return result;
            }

            return InsertMissingToken(kind);
        }

        private bool EvaluateBool(ExpressionSyntax expr)
        {
            var result = Evaluate(expr);
            if (result is bool)
                return (bool)result;
            if (result is int)
                return (int)result != 0;

            return false;
        }

        private int EvaluateInt(ExpressionSyntax expr)
        {
            var result = Evaluate(expr);
            if (result is int)
                return (int)result;

            return 0;
        }

        private object Evaluate(ExpressionSyntax expr)
        {
            if (expr == null)
                return null;

            switch (expr.Kind)
            {
                case SyntaxKind.ParenthesizedExpression:
                    return Evaluate(((ParenthesizedExpressionSyntax)expr).Expression);
                case SyntaxKind.TrueLiteralExpression:
                case SyntaxKind.FalseLiteralExpression:
                case SyntaxKind.NumericLiteralExpression:
                    return ((LiteralExpressionSyntax)expr).Token.Value;
                case SyntaxKind.LogicalAndExpression:
                case SyntaxKind.BitwiseAndExpression:
                    return EvaluateBool(((BinaryExpressionSyntax)expr).Left) && EvaluateBool(((BinaryExpressionSyntax)expr).Right);
                case SyntaxKind.LogicalOrExpression:
                case SyntaxKind.BitwiseOrExpression:
                    return EvaluateBool(((BinaryExpressionSyntax)expr).Left) || EvaluateBool(((BinaryExpressionSyntax)expr).Right);
                case SyntaxKind.EqualsExpression:
                    return Equals(Evaluate(((BinaryExpressionSyntax)expr).Left), Evaluate(((BinaryExpressionSyntax)expr).Right));
                case SyntaxKind.NotEqualsExpression:
                    return !Equals(Evaluate(((BinaryExpressionSyntax)expr).Left), Evaluate(((BinaryExpressionSyntax)expr).Right));
                case SyntaxKind.LogicalNotExpression:
                    return !EvaluateBool(((PrefixUnaryExpressionSyntax)expr).Operand);
                case SyntaxKind.AddExpression:
                    return EvaluateInt(((BinaryExpressionSyntax)expr).Left) + EvaluateInt(((BinaryExpressionSyntax)expr).Right);
                case SyntaxKind.SubtractExpression:
                    return EvaluateInt(((BinaryExpressionSyntax)expr).Left) - EvaluateInt(((BinaryExpressionSyntax)expr).Right);
                case SyntaxKind.MultiplyExpression:
                    return EvaluateInt(((BinaryExpressionSyntax)expr).Left) * EvaluateInt(((BinaryExpressionSyntax)expr).Right);
                case SyntaxKind.DivideExpression:
                    var divisor = EvaluateInt(((BinaryExpressionSyntax) expr).Right);
                    return (divisor != 0)
                        ? EvaluateInt(((BinaryExpressionSyntax) expr).Left) / divisor
                        : int.MaxValue;
                case SyntaxKind.GreaterThanExpression:
                    return EvaluateInt(((BinaryExpressionSyntax)expr).Left) > EvaluateInt(((BinaryExpressionSyntax)expr).Right);
                case SyntaxKind.GreaterThanOrEqualExpression:
                    return EvaluateInt(((BinaryExpressionSyntax)expr).Left) >= EvaluateInt(((BinaryExpressionSyntax)expr).Right);
                case SyntaxKind.LessThanExpression:
                    return EvaluateInt(((BinaryExpressionSyntax)expr).Left) < EvaluateInt(((BinaryExpressionSyntax)expr).Right);
                case SyntaxKind.LessThanOrEqualExpression:
                    return EvaluateInt(((BinaryExpressionSyntax)expr).Left) <= EvaluateInt(((BinaryExpressionSyntax)expr).Right);
                case SyntaxKind.IdentifierName:
                    var id = ((IdentifierNameSyntax)expr).Name.Text;
                    return IsDirectiveDefined(id);
                case SyntaxKind.FunctionInvocationExpression:
                    // It must be a call to "defined" - that's the only one allowed by the parser.
                    var functionCall = (FunctionInvocationExpressionSyntax)expr;
                    var identifierName = ((IdentifierNameSyntax)functionCall.ArgumentList.Arguments[0]).Name;
                    return IsDirectiveDefined(identifierName.Text);
                default:
                    return false;
            }
        }

        private bool IsDirectiveDefined(string id)
        {
            var defState = _directiveStack.IsDefined(id);
            switch (defState)
            {
                case DefineState.Defined:
                    return true;
                default:
                    return false;
            }
        }
    }
}