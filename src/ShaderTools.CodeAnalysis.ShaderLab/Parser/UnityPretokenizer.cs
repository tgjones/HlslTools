using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Parser;
using ShaderTools.CodeAnalysis.ShaderLab.Diagnostics;
using ShaderTools.CodeAnalysis.ShaderLab.Syntax;

namespace ShaderTools.CodeAnalysis.ShaderLab.Parser
{
    internal sealed class UnityPretokenizer
    {
        private readonly List<PretokenizedSyntaxTrivia> _leadingTrivia = new List<PretokenizedSyntaxTrivia>();
        private readonly List<PretokenizedSyntaxTrivia> _trailingTrivia = new List<PretokenizedSyntaxTrivia>();
        private readonly List<PretokenizedDiagnostic> _diagnostics = new List<PretokenizedDiagnostic>();

        private SyntaxKind _kind;
        private SyntaxKind _contextualKind;
        private object _value;
        private int _lexemeStart;
        private int _start;

        private readonly CharReader _charReader;

        public SourceText Text { get; }

        public UnityPretokenizer(SourceText text)
        {
            _charReader = new CharReader(text);
            Text = text;
        }

        // TODO: Constructor that takes an existing array of pretokenized tokens,
        // along with new text and changes.

        public ImmutableArray<PretokenizedSyntaxToken> GetTokens()
        {
            var tokens = ImmutableArray.CreateBuilder<PretokenizedSyntaxToken>();

            PretokenizedSyntaxToken token;
            do
            {
                tokens.Add(token = LexSyntaxToken());
            } while (token.RawKind != (ushort) SyntaxKind.EndOfFileToken);

            return tokens.ToImmutable();
        }

        private void NextChar()
        {
            _charReader.NextChar();
        }

        private PretokenizedSyntaxToken LexSyntaxToken()
        {
            _lexemeStart = _charReader.Position;

            _diagnostics.Clear();
            _leadingTrivia.Clear();
            _start = _charReader.Position;

            if (_kind == SyntaxKind.CgProgramKeyword || _kind == SyntaxKind.CgIncludeKeyword)
                ReadCgTrivia(_leadingTrivia);

            ReadTrivia(_leadingTrivia, isTrailing: false);
            var leadingTrivia = _leadingTrivia.ToImmutableArray();

            _kind = SyntaxKind.BadToken;
            _contextualKind = SyntaxKind.None;
            _value = null;
            _diagnostics.Clear();
            _start = _charReader.Position;
            ReadToken();
            var end = _charReader.Position;
            var kind = _kind;
            var span = TextSpan.FromBounds(_start, end);
            var text = Text.GetSubText(span).ToString();
            var diagnostics = GetErrors(leadingTriviaWidth: GetFullWidth(leadingTrivia));

            _trailingTrivia.Clear();
            _diagnostics.Clear();
            _start = _charReader.Position;
            ReadTrivia(_trailingTrivia, isTrailing: true);
            var trailingTrivia = _trailingTrivia.ToImmutableArray();

            return new PretokenizedSyntaxToken(
                (ushort) kind, 
                (ushort) _contextualKind,
                text, 
                _value,
                leadingTrivia, 
                trailingTrivia, 
                diagnostics);
        }

        private static int GetFullWidth(ImmutableArray<PretokenizedSyntaxTrivia> trivia)
        {
            var width = 0;
            for (int i = 0; i < trivia.Length; i++)
                width += trivia[i].FullWidth;
            return width;
        }

        private void ReadCgTrivia(List<PretokenizedSyntaxTrivia> target)
        {
            while (_charReader.Current != '\0')
            {
                if (_charReader.Peek(0) == 'E'
                    && _charReader.Peek(1) == 'N'
                    && _charReader.Peek(2) == 'D'
                    && _charReader.Peek(3) == 'C'
                    && _charReader.Peek(4) == 'G')
                {
                    break;
                }
                NextChar();
            }
            AddTrivia(target, SyntaxKind.CgProgramTrivia);
        }

        private ImmutableArray<PretokenizedDiagnostic> GetErrors(int leadingTriviaWidth)
        {
            if (_diagnostics != null)
            {
                if (leadingTriviaWidth > 0)
                {
                    var array = new PretokenizedDiagnostic[_diagnostics.Count];
                    for (int i = 0; i < _diagnostics.Count; i++)
                    {
                        // fixup error positioning to account for leading trivia
                        array[i] = _diagnostics[i].WithOffset(_diagnostics[i].Offset + leadingTriviaWidth);
                    }

                    return array.ToImmutableArray();
                }
                else
                {
                    return _diagnostics.ToImmutableArray();
                }
            }
            else
            {
                return ImmutableArray<PretokenizedDiagnostic>.Empty;
            }
        }

        private void ReadTrivia(List<PretokenizedSyntaxTrivia> target, bool isTrailing)
        {
            while (true)
            {
                switch (_charReader.Current)
                {
                    case '\n':
                    case '\r':
                        {
                            ReadEndOfLine();
                            AddTrivia(target, SyntaxKind.EndOfLineTrivia);
                            if (isTrailing)
                                return;
                        }
                        break;
                    case '/':
                        if (_charReader.Peek() == '/')
                        {
                            ReadSinglelineComment();
                            AddTrivia(target, SyntaxKind.SingleLineCommentTrivia);
                        }
                        else if (_charReader.Peek() == '*')
                        {
                            ReadMultilineComment();
                            AddTrivia(target, SyntaxKind.MultiLineCommentTrivia);
                        }
                        else
                        {
                            return;
                        }
                        break;

                    default:
                        if (char.IsWhiteSpace(_charReader.Current))
                        {
                            ReadWhitespace();
                            AddTrivia(target, SyntaxKind.WhitespaceTrivia);
                        }
                        else
                        {
                            return;
                        }
                        break;
                }
            }
        }

        private void ReadEndOfLine()
        {
            if (_charReader.Current == '\r')
            {
                NextChar();

                if (_charReader.Current == '\n')
                    NextChar();
            }
            else
            {
                NextChar();
            }
        }

        private void ReadSinglelineComment()
        {
            _kind = SyntaxKind.SingleLineCommentTrivia;
            while (true)
            {
                switch (_charReader.Current)
                {
                    case '\0':
                        return;

                    case '\r':
                    case '\n':
                        return;

                    default:
                        NextChar();
                        break;
                }
            }
        }

        private void ReadMultilineComment()
        {
            NextChar(); // Skip /
            NextChar(); // Skip *

            _kind = SyntaxKind.MultiLineCommentTrivia;

            while (true)
            {
                switch (_charReader.Current)
                {
                    case '\0':
                        _diagnostics.ReportUnterminatedComment();
                        return;

                    case '*':
                        NextChar();
                        if (_charReader.Current == '/')
                        {
                            NextChar();
                            return;
                        }
                        break;

                    default:
                        NextChar();
                        break;
                }
            }
        }

        private void ReadWhitespace()
        {
            while (char.IsWhiteSpace(_charReader.Current) &&
                   _charReader.Current != '\r' &&
                   _charReader.Current != '\n')
            {
                NextChar();
            }
        }

        private void AddTrivia(List<PretokenizedSyntaxTrivia> target, SyntaxKind kind)
        {
            var start = _start;
            var end = _charReader.Position;
            var span = TextSpan.FromBounds(start, end);
            var text = Text.GetSubText(span).ToString();
            var diagnostics = GetErrors(leadingTriviaWidth: 0);
            var trivia = new PretokenizedSyntaxTrivia((ushort) kind, text, diagnostics);
            target.Add(trivia);

            _diagnostics.Clear();
            _start = _charReader.Position;
        }

        private void ReadToken()
        {
            switch (_charReader.Current)
            {
                case '\0':
                    _kind = SyntaxKind.EndOfFileToken;
                    break;

                case '~':
                    _kind = SyntaxKind.TildeToken;
                    NextChar();
                    break;

                case '&':
                    NextChar();
                    if (_charReader.Current == '&')
                    {
                        _kind = SyntaxKind.AmpersandAmpersandToken;
                        NextChar();
                    }
                    else if (_charReader.Current == '=')
                    {
                        _kind = SyntaxKind.AmpersandEqualsToken;
                        NextChar();
                    }
                    else
                    {
                        _kind = SyntaxKind.AmpersandToken;
                    }
                    break;

                case '|':
                    NextChar();
                    if (_charReader.Current == '|')
                    {
                        _kind = SyntaxKind.BarBarToken;
                        NextChar();
                    }
                    else if (_charReader.Current == '=')
                    {
                        _kind = SyntaxKind.BarEqualsToken;
                        NextChar();
                    }
                    else
                    {
                        _kind = SyntaxKind.BarToken;
                    }
                    break;

                case '^':
                    NextChar();
                    if (_charReader.Current == '=')
                    {
                        _kind = SyntaxKind.CaretEqualsToken;
                        NextChar();
                    }
                    else
                    {
                        _kind = SyntaxKind.CaretToken;
                    }
                    break;

                case '?':
                    _kind = SyntaxKind.QuestionToken;
                    NextChar();
                    break;

                case '(':
                    _kind = SyntaxKind.OpenParenToken;
                    NextChar();
                    break;

                case ')':
                    _kind = SyntaxKind.CloseParenToken;
                    NextChar();
                    break;

                case '[':
                    _kind = SyntaxKind.OpenBracketToken;
                    NextChar();
                    break;

                case ']':
                    _kind = SyntaxKind.CloseBracketToken;
                    NextChar();
                    break;

                case '{':
                    _kind = SyntaxKind.OpenBraceToken;
                    NextChar();
                    break;

                case '}':
                    _kind = SyntaxKind.CloseBraceToken;
                    NextChar();
                    break;

                case '.':
                    if (char.IsDigit(_charReader.Peek()))
                        ReadNumber();
                    else
                    {
                        _kind = SyntaxKind.DotToken;
                        NextChar();
                    }
                    break;

                case '+':
                    NextChar();
                    if (_charReader.Current == '=')
                    {
                        _kind = SyntaxKind.PlusEqualsToken;
                        NextChar();
                    }
                    else if (_charReader.Current == '+')
                    {
                        _kind = SyntaxKind.PlusPlusToken;
                        NextChar();
                    }
                    else
                    {
                        _kind = SyntaxKind.PlusToken;
                    }
                    break;

                case '-':
                    NextChar();
                    if (_charReader.Current == '=')
                    {
                        _kind = SyntaxKind.MinusEqualsToken;
                        NextChar();
                    }
                    else if (_charReader.Current == '-')
                    {
                        _kind = SyntaxKind.MinusMinusToken;
                        NextChar();
                    }
                    else
                    {
                        _kind = SyntaxKind.MinusToken;
                    }
                    break;

                case '*':
                    NextChar();
                    if (_charReader.Current == '=')
                    {
                        _kind = SyntaxKind.AsteriskEqualsToken;
                        NextChar();
                    }
                    else
                    {
                        _kind = SyntaxKind.AsteriskToken;
                    }
                    break;

                case '/':
                    NextChar();
                    if (_charReader.Current == '=')
                    {
                        _kind = SyntaxKind.SlashEqualsToken;
                        NextChar();
                    }
                    else
                    {
                        _kind = SyntaxKind.SlashToken;
                    }
                    break;

                case '%':
                    NextChar();
                    if (_charReader.Current == '=')
                    {
                        _kind = SyntaxKind.PercentEqualsToken;
                        NextChar();
                    }
                    else
                    {
                        _kind = SyntaxKind.PercentToken;
                    }
                    break;

                case ',':
                    _kind = SyntaxKind.CommaToken;
                    NextChar();
                    break;

                case ';':
                    _kind = SyntaxKind.SemiToken;
                    NextChar();
                    break;

                case ':':
                    NextChar();
                    if (_charReader.Current == ':')
                    {
                        _kind = SyntaxKind.ColonColonToken;
                        NextChar();
                    }
                    else
                    {
                        _kind = SyntaxKind.ColonToken;
                    }
                    break;

                case '=':
                    NextChar();
                    if (_charReader.Current == '=')
                    {
                        _kind = SyntaxKind.EqualsEqualsToken;
                        NextChar();
                    }
                    else
                    {
                        _kind = SyntaxKind.EqualsToken;
                    }
                    break;

                case '!':
                    NextChar();
                    if (_charReader.Current == '=')
                    {
                        _kind = SyntaxKind.ExclamationEqualsToken;
                        NextChar();
                    }
                    else
                    {
                        _kind = SyntaxKind.NotToken;
                    }
                    break;

                case '<':
                    NextChar();
                    if (_charReader.Current == '=')
                    {
                        _kind = SyntaxKind.LessThanEqualsToken;
                        NextChar();
                    }
                    else if (_charReader.Current == '<')
                    {
                        NextChar();
                        if (_charReader.Current == '=')
                        {
                            _kind = SyntaxKind.LessThanLessThanEqualsToken;
                            NextChar();
                        }
                        else
                        {
                            _kind = SyntaxKind.LessThanLessThanToken;
                        }
                    }
                    else
                    {
                        _kind = SyntaxKind.LessThanToken;
                    }
                    break;

                case '>':
                    NextChar();
                    if (_charReader.Current == '=')
                    {
                        _kind = SyntaxKind.GreaterThanEqualsToken;
                        NextChar();
                    }
                    else if (_charReader.Current == '>')
                    {
                        NextChar();
                        if (_charReader.Current == '=')
                        {
                            _kind = SyntaxKind.GreaterThanGreaterThanEqualsToken;
                            NextChar();
                        }
                        else
                        {
                            _kind = SyntaxKind.GreaterThanGreaterThanToken;
                        }
                    }
                    else
                    {
                        _kind = SyntaxKind.GreaterThanToken;
                    }
                    break;

                case '"':
                    ReadString();
                    break;

                default:
                    if (char.IsLetter(_charReader.Current) || _charReader.Current == '_' || (char.IsDigit(_charReader.Current) && _charReader.Peek() == 'D'))
                        ReadIdentifierOrKeyword();
                    else if (char.IsDigit(_charReader.Current))
                        ReadNumber();
                    else
                        ReadInvalidCharacter();

                    break;
            }
        }

        private void ReadInvalidCharacter()
        {
            var c = _charReader.Current;
            NextChar();
            _diagnostics.ReportIllegalInputCharacter(c);
        }

        private void ReadString()
        {
            _kind = SyntaxKind.StringLiteralToken;

            // Skip first double quote
            NextChar();

            var sb = new StringBuilder();

            while (true)
            {
                switch (_charReader.Current)
                {
                    case '\0':
                        _diagnostics.ReportUnterminatedString();
                        goto ExitLoop;

                    case '\\':
                        sb.Append(_charReader.Current);
                        NextChar();
                        sb.Append(_charReader.Current);
                        NextChar();
                        break;

                    case '"':
                        NextChar();

                        if (_charReader.Current != '"')
                            goto ExitLoop;

                        sb.Append(_charReader.Current);
                        NextChar();
                        break;

                    default:
                        sb.Append(_charReader.Current);
                        NextChar();
                        break;
                }
            }

        ExitLoop:
            _value = sb.ToString();
        }

        private void ReadNumber()
        {
            // Just read everything that looks like it could be a number -- we will
            // verify it afterwards by proper number parsing.

            var sb = new StringBuilder();
            var hasOctalPrefix = false;
            var hasExponentialModifier = false;
            var hasDotModifier = false;
            var hasFloatSuffix = false;
            var hasHexModifier = false;

            while (true)
            {
                switch (_charReader.Current)
                {
                    // dot
                    case '.':
                        if (hasHexModifier || hasOctalPrefix)
                            goto ExitLoop;
                        sb.Append(_charReader.Current);
                        NextChar();
                        hasDotModifier = true;
                        break;

                    // special handling for e, it could be the exponent indicator
                    // followed by an optional sign

                    case 'E':
                    case 'e':
                        if (hasHexModifier)
                            goto case '0';

                        sb.Append(_charReader.Current);
                        NextChar();
                        hasExponentialModifier = true;
                        if (_charReader.Current == '-' || _charReader.Current == '+')
                        {
                            sb.Append(_charReader.Current);
                            NextChar();
                        }
                        break;

                    case 'F':
                    case 'f':
                    case 'H':
                    case 'h':
                        if (hasHexModifier)
                            goto case '0';
                        hasFloatSuffix = true;
                        NextChar();
                        goto ExitLoop;

                    case 'L':
                    case 'l':
                    case 'U':
                    case 'u':
                        var currentSuffix = _charReader.Current;
                        NextChar();
                        if ((currentSuffix == 'U' || currentSuffix == 'u')
                            && (_charReader.Current == 'L' || _charReader.Current == 'l'))
                        {
                            NextChar();
                        }
                        else if ((currentSuffix == 'L' || currentSuffix == 'l')
                            && (_charReader.Current == 'U' || _charReader.Current == 'u'))
                        {
                            NextChar();
                        }
                        goto ExitLoop;

                    case '#':
                        if (sb.ToString() != "1.")
                            goto ExitLoop;
                        if (_charReader.Peek(1) == 'I' && _charReader.Peek(2) == 'N'
                            && (_charReader.Peek(3) == 'D' || _charReader.Peek(3) == 'F'))
                        {
                            var isInfinity = _charReader.Peek(3) == 'F';

                            NextChar();
                            NextChar();
                            NextChar();
                            NextChar();

                            _kind = SyntaxKind.FloatLiteralToken;
                            _value = isInfinity ? float.PositiveInfinity : float.NaN;
                            return;
                        }
                        goto ExitLoop;

                    case 'X':
                    case 'x':
                        hasHexModifier = true;
                        sb.Append(_charReader.Current);
                        NextChar();
                        break;

                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        if (_charReader.Current == '0' && sb.Length == 0)
                        {
                            switch (_charReader.Peek())
                            {
                                case '0':
                                case '1':
                                case '2':
                                case '3':
                                case '4':
                                case '5':
                                case '6':
                                case '7':
                                    hasOctalPrefix = true;
                                    break;
                            }

                            sb.Append(_charReader.Current);
                            NextChar();
                            break;
                        }

                        if (hasOctalPrefix)
                        {
                            switch (_charReader.Current)
                            {
                                case '0':
                                case '1':
                                case '2':
                                case '3':
                                case '4':
                                case '5':
                                case '6':
                                case '7':
                                    sb.Append(_charReader.Current);
                                    NextChar();
                                    break;

                                default:
                                    goto ExitLoop;
                            }
                        }
                        else
                        {
                            sb.Append(_charReader.Current);
                            NextChar();
                        }
                        break;

                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                        if (hasHexModifier)
                            goto case '0';
                        goto default;

                    default:
                        goto ExitLoop;
                }
            }

        ExitLoop:

            var text = sb.ToString();

            if ((hasDotModifier || hasExponentialModifier || hasFloatSuffix) && !hasHexModifier)
                _value = ReadDouble(text);
            else
                _value = ReadInt32OrInt64(text, hasHexModifier, hasOctalPrefix);
        }

        private double ReadDouble(string text)
        {
            _kind = SyntaxKind.FloatLiteralToken;

            try
            {
                return double.Parse(text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
            }
            catch (OverflowException)
            {
                _diagnostics.ReportNumberTooLarge(text);
            }
            catch (FormatException)
            {
                _diagnostics.ReportInvalidReal(text);
            }
            return 0.0;
        }

        private object ReadInt32OrInt64(string text, bool hasHexModifier, bool hasOctalPrefix)
        {
            _kind = SyntaxKind.IntegerLiteralToken;

            var int64 = ReadInt64(text, hasHexModifier, hasOctalPrefix);

            // If the integer can be represented as Int32 we return
            // an Int32 literal. Otherwise we return an Int64.

            if (int64 >= int.MinValue && int64 <= int.MaxValue)
                return (int) int64;

            return int64;
        }

        private long ReadInt64(string text, bool hasHexModifier, bool hasOctalPrefix)
        {
            if (hasHexModifier)
            {
                try
                {
                    return Convert.ToInt64(text, 16);
                }
                catch (OverflowException)
                {
                    _diagnostics.ReportNumberTooLarge(text);
                }
                catch (FormatException)
                {
                    _diagnostics.ReportInvalidHex(text);
                }

                return 0;
            }

            if (hasOctalPrefix)
            {
                try
                {
                    return Convert.ToInt64(text, 8);
                }
                catch (OverflowException)
                {
                    _diagnostics.ReportNumberTooLarge(text);
                }
                catch (FormatException)
                {
                    _diagnostics.ReportInvalidHex(text);
                }

                return 0;
            }

            try
            {
                return long.Parse(text, CultureInfo.InvariantCulture);
            }
            catch (OverflowException)
            {
                _diagnostics.ReportNumberTooLarge(text);
            }
            catch (FormatException)
            {
                _diagnostics.ReportInvalidInteger(text);
            }

            return 0;
        }

        private long ReadOctalValue(string octal)
        {
            long val = 0;

            for (int i = octal.Length - 1, j = 0; i >= 0; i--, j++)
            {
                int c;

                try
                {
                    c = int.Parse(new string(octal[i], 1), CultureInfo.InvariantCulture);

                    if (c > 7)
                    {
                        _diagnostics.ReportInvalidOctal(octal);
                        return 0;
                    }
                }
                catch (FormatException)
                {
                    _diagnostics.ReportInvalidOctal(octal);
                    return 0;
                }

                checked
                {
                    val += (long) (c * Math.Pow(8, j));
                }
            }

            return val;
        }

        private void ReadIdentifierOrKeyword()
        {
            var start = _charReader.Position;

            // Skip first letter
            NextChar();

            // The following characters can be letters, digits the underscore and the dollar sign.

            while (char.IsLetterOrDigit(_charReader.Current) ||
                   _charReader.Current == '_' ||
                   _charReader.Current == '$')
            {
                NextChar();
            }

            var end = _charReader.Position;
            var span = TextSpan.FromBounds(start, end);
            var text = Text.GetSubText(span).ToString();

            _kind = SyntaxFacts.GetUnityKeywordKind(text);

            _contextualKind = SyntaxFacts.GetContextualKeywordKind(text);

            switch (_kind)
            {
                case SyntaxKind.TrueKeyword:
                    _value = true;
                    break;
                case SyntaxKind.FalseKeyword:
                    _value = false;
                    break;
                default:
                    _value = text;
                    break;
            }
        }
    }
}
