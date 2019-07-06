using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Parser
{
    public partial class HlslLexer
    {
        private readonly Stack<DefineDirectiveTriviaSyntax> _currentlyExpandingMacros = new Stack<DefineDirectiveTriviaSyntax>();

        private bool TryExpandMacro(SyntaxToken token, IMacroExpansionLexer lexer, out List<SyntaxToken> expandedTokens)
        {
            expandedTokens = null;

            // First, check if this token might be a macro.
            DefineDirectiveTriviaSyntax directive;
            if (_directives.IsDefined(token.Text, out directive) != DefineState.Defined)
                return false;

            // Check that this macro is not disabled.
            if (_currentlyExpandingMacros.Contains(directive))
                return false;

            MacroReference macroReference;
            List<SyntaxToken> macroBody;
            SyntaxToken lastToken;
            switch (directive.Kind)
            {
                case SyntaxKind.FunctionLikeDefineDirectiveTrivia:
                    // For function-like macros, check for, and expand, macro arguments.
                    var functionLikeDefine = (FunctionLikeDefineDirectiveTriviaSyntax)directive;

                    // ... check to see if the next token is an open paren.
                    if (lexer.Peek(_mode).Kind != SyntaxKind.OpenParenToken)
                        return false;

                    // If it is, then parse the macro arguments, and
                    // check that we have the correct number of arguments.
                    ExpandMacros = false;
                    var macroArguments = new MacroArgumentsParser(lexer).ParseArgumentList();
                    ExpandMacros = true;

                    if (macroArguments.Arguments.Count != functionLikeDefine.Parameters.Parameters.Count)
                    {
                        expandedTokens = new List<SyntaxToken>
                        {
                            token
                                .WithDiagnostic(Diagnostic.Create(HlslMessageProvider.Instance, token.SourceRange, (int) DiagnosticId.NotEnoughMacroParameters, token.Text))
                                .WithTrailingTrivia(new[] { macroArguments })
                        };
                        return true;
                    }

                    var functionLikeDefineDirective = (FunctionLikeDefineDirectiveTriviaSyntax)directive;
                    macroReference = new FunctionLikeMacroReference(token, macroArguments, functionLikeDefineDirective);

                    // Expand arguments.
                    var expandedArguments = macroArguments.Arguments
                        .Select(x => ExpandNestedMacro(new NestedMacroExpansionLexer(x.Tokens)).ToList())
                        .ToList();

                    // Replace parameters with possibly-expanded arguments.
                    macroBody = ReplaceParameters(
                        macroArguments.Arguments.ToList(), expandedArguments,
                        functionLikeDefineDirective.Parameters,
                        functionLikeDefineDirective.Body);

                    lastToken = macroArguments.DescendantTokens().LastOrDefault(x => !x.IsMissing) ?? token;

                    break;

                case SyntaxKind.ObjectLikeDefineDirectiveTrivia:
                    macroReference = new ObjectLikeMacroReference(token, (ObjectLikeDefineDirectiveTriviaSyntax)directive);
                    macroBody = directive.MacroBody;
                    lastToken = token;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (_mode)
            {
                case LexerMode.Syntax:
                    // Push the current macro onto the stack - this prevents recursive expansion.
                    _currentlyExpandingMacros.Push(directive);

                    if (_mode == LexerMode.Syntax)
                    {
                        // Scan macro body for nested macros.
                        expandedTokens = ExpandNestedMacro(new NestedMacroExpansionLexer(macroBody));

                        // Relex identifier tokens, because at this point keywords are stored as identifiers.
                        for (var i = 0; i < expandedTokens.Count; i++)
                            if (expandedTokens[i].Kind == SyntaxKind.IdentifierToken)
                            {
                                var relexedToken = new HlslLexer(new SourceFile(SourceText.From(expandedTokens[i].Text))).Lex(LexerMode.Syntax);
                                expandedTokens[i] = expandedTokens[i].WithKind(relexedToken.Kind).WithContextualKind(relexedToken.ContextualKind);
                            }

                        var localExpandedTokens = expandedTokens;
                        expandedTokens = expandedTokens
                            .Select((x, i) =>
                            {
                                var result = x
                                    .WithOriginalMacroReference(macroReference, i == 0)
                                    .WithSpan(macroReference.SourceRange, macroReference.FileSpan);
                                if (i == 0)
                                    result = result.WithLeadingTrivia(token.LeadingTrivia);
                                if (i == localExpandedTokens.Count - 1)
                                    result = result.WithTrailingTrivia(lastToken.TrailingTrivia);
                                return result;
                            })
                            .ToList();
                    }

                    _currentlyExpandingMacros.Pop();
                    break;

                case LexerMode.Directive: // If we're in the middle of parsing a directive, don't expand macro references.
                    expandedTokens = macroBody
                        .Select((x, i) => x.WithOriginalMacroReference(macroReference, i == 0).WithSpan(macroReference.SourceRange, macroReference.FileSpan))
                        .ToList();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return true;
        }

        private List<SyntaxToken> ExpandNestedMacro(NestedMacroExpansionLexer lexer)
        {
            var result = new List<SyntaxToken>();

            SyntaxToken token;
            while ((token = lexer.GetNextToken()) != null)
            {
                List<SyntaxToken> expandedTokens;
                if (TryExpandMacro(token, lexer, out expandedTokens))
                    result.AddRange(expandedTokens);
                else
                    result.Add(token);
            }

            return result;
        }

        private static List<SyntaxToken> ReplaceParameters(
            List<MacroArgumentSyntax> originalArguments,
            List<List<SyntaxToken>> expandedArguments,
            FunctionLikeDefineDirectiveParameterListSyntax parameterList,
            List<SyntaxToken> macroBody)
        {
            var parameters = parameterList.Parameters;

            var result = new List<SyntaxToken>();

            for (var i = 0; i < macroBody.Count; i++)
            {
                var token = macroBody[i];

                // Do token pasting (##).
                if (i < macroBody.Count - 2
                    && macroBody[i + 1].Kind == SyntaxKind.HashHashToken
                    && macroBody[i + 2].Kind != SyntaxKind.EndOfFileToken
                    && macroBody[i + 2].Kind != SyntaxKind.HashHashToken)
                {
                    var parameterIndexLeft = FindParameterIndex(parameters, token);
                    var pastedText = (parameterIndexLeft != -1) ? originalArguments[parameterIndexLeft].ToString(true) : token.Text;

                    while (i < macroBody.Count - 2
                        && macroBody[i + 1].Kind == SyntaxKind.HashHashToken
                        && macroBody[i + 2].Kind != SyntaxKind.EndOfFileToken
                        && macroBody[i + 2].Kind != SyntaxKind.HashHashToken)
                    {
                        var parameterIndexRight = FindParameterIndex(parameters, macroBody[i + 2]);
                        var textRight = (parameterIndexRight != -1) ? originalArguments[parameterIndexRight].ToString(true) : macroBody[i + 2].Text;
                        pastedText += textRight;
                        i += 2;
                    }
                    result.AddRange(SyntaxFactory.ParseAllTokens(new SourceFile(SourceText.From(pastedText))).TakeWhile(t => t.Kind != SyntaxKind.EndOfFileToken));
                    continue;
                }

                switch (token.Kind)
                {
                    // Potentially stringify.
                    case SyntaxKind.HashToken:
                        {
                            if (i < macroBody.Count - 1 && macroBody[i + 1].Kind.IsIdentifierOrKeyword())
                            {
                                var parameterIndex = FindParameterIndex(parameters, macroBody[i + 1]);
                                if (parameterIndex != -1)
                                {
                                    var stringifiedText = "\"" + originalArguments[parameterIndex].ToString(true) + "\"";
                                    result.Add(SyntaxFactory.ParseToken(stringifiedText));
                                    i++;
                                    break;
                                }
                            }
                            goto default;
                        }

                    // Potentially replacement parameter with argument tokens.
                    case SyntaxKind.IdentifierToken:
                        {
                            var parameterIndex = FindParameterIndex(parameters, token);
                            if (parameterIndex != -1)
                                result.AddRange(expandedArguments[parameterIndex]);
                            else
                                result.Add(token);
                            break;
                        }

                    default:
                        {
                            if (token.Kind.IsKeyword())
                                goto case SyntaxKind.IdentifierToken;
                            result.Add(token);
                            break;
                        }
                }
            }

            return result;
        }

        private static int FindParameterIndex(SeparatedSyntaxList<SyntaxToken> parameters, SyntaxToken token)
        {
            var parameterIndex = -1;
            for (var index = 0; index < parameters.Count; index++)
            {
                var parameter = parameters[index];
                if (parameter.Text == token.Text)
                {
                    parameterIndex = index;
                    break;
                }
            }
            return parameterIndex;
        }

        private interface IMacroExpansionLexer : ILexer
        {
            SyntaxToken Peek(LexerMode mode);
        }

        private class BaseMacroExpansionLexer : IMacroExpansionLexer
        {
            private readonly HlslLexer _lexer;

            public BaseMacroExpansionLexer(HlslLexer lexer)
            {
                _lexer = lexer;
            }

            //public SourceText Text => _lexer.Text;

            public SyntaxToken Lex(LexerMode mode) => _lexer.Lex(mode);

            public SyntaxToken Peek(LexerMode mode)
            {
                var currentPosition = _lexer._charReader.Position;

                var result = Lex(mode);

                _lexer._charReader.Reset(currentPosition);

                return result;
            }
        }

        private class NestedMacroExpansionLexer : IMacroExpansionLexer
        {
            private readonly List<SyntaxToken> _macroBody;
            private int _tokenIndex;

            public NestedMacroExpansionLexer(List<SyntaxToken> macroBody)
            {
                _macroBody = macroBody;
                _tokenIndex = 0;
            }

            public SourceText Text
            {
                get { throw new NotSupportedException(); }
            }

            public SyntaxToken Lex(LexerMode mode)
            {
                return (_tokenIndex < _macroBody.Count)
                    ? _macroBody[_tokenIndex++]
                    : SyntaxFactory.ParseToken("\0");
            }

            public SyntaxToken GetNextToken()
            {
                if (_tokenIndex < _macroBody.Count)
                    return _macroBody[_tokenIndex++];
                return null;
            }

            public SyntaxToken Peek(LexerMode mode)
            {
                if (_tokenIndex < _macroBody.Count)
                    return _macroBody[_tokenIndex];
                return SyntaxFactory.ParseToken("\0");
            }
        }
    }
}