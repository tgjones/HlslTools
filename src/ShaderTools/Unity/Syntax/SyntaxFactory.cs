using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ShaderTools.Core.Text;
using ShaderTools.Unity.Parser;

namespace ShaderTools.Unity.Syntax
{
    public static class SyntaxFactory
    {
        public static SyntaxTree ParseUnitySyntaxTree(SourceText sourceText, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Parse(
                sourceText,
                p => p.ParseUnityCompilationUnit(cancellationToken));
        }

        private static SyntaxTree Parse(SourceText sourceText, Func<UnityParser, SyntaxNode> parseFunc)
        {
            var lexer = new UnityLexer(sourceText);
            var parser = new UnityParser(lexer);

            var result = new SyntaxTree(sourceText,
                syntaxTree => parseFunc(parser));

            Debug.WriteLine(DateTime.Now + " - Finished parsing");

            return result;
        }

        public static IReadOnlyList<SyntaxToken> ParseAllTokens(SourceText sourceText)
        {
            var tokens = new List<SyntaxToken>();

            var lexer = new UnityLexer(sourceText);
            SyntaxToken token;
            do
            {
                tokens.Add(token = lexer.Lex());
            } while (token.Kind != SyntaxKind.EndOfFileToken);

            return tokens;
        }
    }
}