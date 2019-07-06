using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Hlsl.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Parser
{
    public partial class HlslLexer
    {
        // It's nasty to make the lexer contextual like this, but I'm not sure how else
        // to lex things like this:
        // #include <foo.fxh>
        private SyntaxKind? _currentDirectiveKind;

        // TODO: Remove this.
        internal SyntaxKind? CurrentDirectiveKind => _currentDirectiveKind;

        private SyntaxToken LexDirectiveToken()
        {
            _kind = SyntaxKind.BadToken;
            _contextualKind = SyntaxKind.BadToken;

            _diagnostics.Clear();
            _start = _charReader.Position;

            var trailingTrivia = new List<SyntaxNode>();

            var isEndOfLine = false;

            switch (_charReader.Current)
            {
                case '#':
                    NextChar();
                    if (_charReader.Current == '#')
                    {
                        NextChar();
                        _kind = SyntaxKind.HashHashToken;
                    }
                    else
                    {
                        _kind = SyntaxKind.HashToken;
                        _currentDirectiveKind = null;
                    }
                    break;

                case '\r':
                case '\n':
                    _kind = SyntaxKind.EndOfDirectiveToken;
                    _currentDirectiveKind = null;
                    isEndOfLine = true;
                    break;

                case '\0':
                    _kind = SyntaxKind.EndOfDirectiveToken;
                    _currentDirectiveKind = null;
                    break;

                case '<':
                    if (_currentDirectiveKind != SyntaxKind.IncludeKeyword)
                        goto default;
                    ReadBracketedString();
                    break;

                default:
                    ReadToken();
                    if (_contextualKind.IsPreprocessorDirective())
                        _currentDirectiveKind = _contextualKind;
                    break;
            }

            var end = _charReader.Position;
            var kind = _kind;
            var span = TextSpan.FromBounds(_start, end);
            var text = File.Text.GetSubText(span).ToString();
            var fileSpan = new SourceFileSpan(File, span);
            var diagnostics = _diagnostics.ToImmutableArray();

            LexDirectiveTrailingTrivia(trailingTrivia, kind, isEndOfLine);

            var token = new SyntaxToken(kind, _contextualKind, false, MakeAbsolute(span), fileSpan, text, _value, 
                ImmutableArray<SyntaxNode>.Empty, trailingTrivia.ToImmutableArray(),
                diagnostics, null, false);

            return token;
        }

        private void ReadBracketedString()
        {
            _kind = SyntaxKind.BracketedStringLiteralToken;

            // Skip open angle bracket.
            NextChar();

            var sb = new StringBuilder();

            while (true)
            {
                switch (_charReader.Current)
                {
                    case '\0':
                        _diagnostics.ReportUnterminatedString(CurrentSpanStart);
                        goto ExitLoop;

                    case '>':
                        NextChar();
                        goto ExitLoop;

                    default:
                        if (Path.GetInvalidPathChars().Contains(_charReader.Current))
                        {
                            _diagnostics.ReportUnterminatedString(CurrentSpanStart);
                            goto ExitLoop;
                        }
                        sb.Append(_charReader.Current);
                        NextChar();
                        break;
                }
            }

            ExitLoop:
            _value = sb.ToString();
        }

        private void LexDirectiveTrailingTrivia(List<SyntaxNode> trivia, SyntaxKind kind, bool includeEndOfLine)
        {
            if (kind == SyntaxKind.EndOfDirectiveToken && includeEndOfLine)
            {
                ReadEndOfLine();
                AddTrivia(trivia, SyntaxKind.EndOfLineTrivia);
            }

            if (kind != SyntaxKind.EndOfDirectiveToken)
            {
                while (true)
                {
                    _diagnostics.Clear();
                    _start = _charReader.Position;

                    switch (_charReader.Current)
                    {
                        case ' ':
                        case '\t':
                            ReadWhitespace();
                            AddTrivia(trivia, SyntaxKind.WhitespaceTrivia);
                            break;

                        case '\\':
                            if (_charReader.Peek() != '\r' && _charReader.Peek() != '\n')
                                goto default;
                            _kind = SyntaxKind.BackslashNewlineTrivia;
                            NextChar();
                            ReadEndOfLine();
                            AddTrivia(trivia, SyntaxKind.BackslashNewlineTrivia);
                            break;

                        case '/':
                            if (_charReader.Peek() == '/')
                            {
                                ReadSinglelineComment();
                                AddTrivia(trivia, SyntaxKind.SingleLineCommentTrivia);
                            }
                            else if (_charReader.Peek() == '*')
                            {
                                ReadMultilineComment();
                                AddTrivia(trivia, SyntaxKind.MultiLineCommentTrivia);
                            }
                            goto EXIT_LOOP;
                        default:
                            goto EXIT_LOOP;
                    }
                }
            }

EXIT_LOOP:
            return;
        }
    }
}