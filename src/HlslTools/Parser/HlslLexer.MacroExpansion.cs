using System;
using System.Collections.Generic;
using System.Linq;
using HlslTools.Diagnostics;
using HlslTools.Syntax;
using HlslTools.Text;

namespace HlslTools.Parser
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
                    var functionLikeDefine = (FunctionLikeDefineDirectiveTriviaSyntax) directive;

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
                                .WithDiagnostic(Diagnostic.Format(token.Span, DiagnosticId.NotEnoughMacroParameters, token.Text))
                                .WithTrailingTrivia(new[] { macroArguments })
                        };
                        return true;
                    }

                    var functionLikeDefineDirective = (FunctionLikeDefineDirectiveTriviaSyntax) directive;
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
                    macroReference = new ObjectLikeMacroReference(token, (ObjectLikeDefineDirectiveTriviaSyntax) directive);
                    macroBody = directive.MacroBody;
                    lastToken = token;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Push the current macro onto the stack - this prevents recursive expansion.
            _currentlyExpandingMacros.Push(directive);

            // Scan macro body for nested macros.
            expandedTokens = ExpandNestedMacro(new NestedMacroExpansionLexer(macroBody));

            var localExpandedTokens = expandedTokens;
            expandedTokens = expandedTokens
                .Select((x, i) =>
                {
                    var result = x
                        .WithOriginalMacroReference(macroReference, i == 0)
                        .WithSpan(macroReference.SourceRange, macroReference.Span);
                    if (i == 0)
                        result = result.WithLeadingTrivia(token.LeadingTrivia);
                    if (i == localExpandedTokens.Count - 1)
                        result = result.WithTrailingTrivia(lastToken.TrailingTrivia);
                    return result;
                })
                .ToList();

            _currentlyExpandingMacros.Pop();

            return true;
        }

        private List<SyntaxToken> ExpandNestedMacro(NestedMacroExpansionLexer lexer)
        {
            var result = new List<SyntaxToken>();

            SyntaxToken token;
            while ((token = lexer.GetNextToken()) != null)
            {
                // Do token pasting (##).
                if (lexer.Peek(0).Kind == SyntaxKind.HashHashToken
                    && lexer.Peek(1).Kind != SyntaxKind.EndOfFileToken && lexer.Peek(1).Kind != SyntaxKind.HashHashToken)
                {
                    lexer.GetNextToken();
                    var concatenatedText = token.Text + lexer.GetNextToken().Text;
                    token = new HlslLexer(new StringText(concatenatedText)).Lex(LexerMode.Syntax);
                }

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

                switch (token.Kind)
                {
                    case SyntaxKind.HashToken:
                    {
                        if (i < macroBody.Count - 1 && macroBody[i + 1].Kind == SyntaxKind.IdentifierToken)
                        {
                            var parameterIndex = FindParameterIndex(parameters, macroBody[i + 1]);
                            if (parameterIndex != -1)
                            {
                                var stringifiedText = "\"" + originalArguments[parameterIndex].ToString(true) + "\"";
                                result.Add(new HlslLexer(new StringText(stringifiedText)).Lex(LexerMode.Syntax));
                                i++;
                                break;
                            }
                        }
                        goto default;
                    }

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

            public SourceText Text => _lexer.Text;

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
                get {  throw new NotSupportedException(); }
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
                return Peek(0);
            }

            public SyntaxToken Peek(int offset)
            {
                if (_tokenIndex + offset < _macroBody.Count)
                    return _macroBody[_tokenIndex + offset];
                return SyntaxFactory.ParseToken("\0");
            }
        }
    }
}