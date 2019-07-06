using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.ShaderLab.Parser;

namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public static partial class SyntaxFactory
    {
        public static SyntaxTree ParseUnitySyntaxTree(SourceText sourceText, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Parse(
                sourceText,
                p => p.ParseUnityCompilationUnit(cancellationToken));
        }

        private static SyntaxTree Parse(SourceText sourceText, Func<UnityParser, SyntaxNode> parseFunc)
        {
            var pretokenizer = new UnityPretokenizer(sourceText);
            var pretokenizedTokens = pretokenizer.GetTokens();

            var lexer = new UnityLexer(sourceText, pretokenizedTokens);
            var parser = new UnityParser(lexer);

            var result = new SyntaxTree(sourceText,
                syntaxTree =>
                {
                    var node = parseFunc(parser);
                    node.SetSyntaxTree(syntaxTree);
                    return node;
                });

            Debug.WriteLine(DateTime.Now + " - Finished parsing");

            return result;
        }

        public static IReadOnlyList<SyntaxToken> ParseAllTokens(SourceText sourceText)
        {
            var tokens = new List<SyntaxToken>();

            var pretokenizer = new UnityPretokenizer(sourceText);
            var pretokenizedTokens = pretokenizer.GetTokens();

            var lexer = new UnityLexer(sourceText, pretokenizedTokens);
            SyntaxToken token;
            do
            {
                tokens.Add(token = lexer.Lex());
            } while (token.Kind != SyntaxKind.EndOfFileToken);

            return tokens;
        }
    }
}