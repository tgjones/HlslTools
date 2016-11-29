using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ShaderTools.Hlsl.Parser;
using ShaderTools.Hlsl.Text;

namespace ShaderTools.Hlsl.Syntax
{
    public static class SyntaxFactory
    {
        public static SyntaxTree ParseSyntaxTree(SourceText sourceText, ParserOptions options = null, IIncludeFileSystem fileSystem = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Parse(sourceText, options, fileSystem ?? new DummyFileSystem(), p => p.ParseCompilationUnit(cancellationToken));
        }

        public static CompilationUnitSyntax ParseCompilationUnit(string text, IIncludeFileSystem fileSystem = null)
        {
            return (CompilationUnitSyntax) Parse(SourceText.From(text), null, fileSystem, p => p.ParseCompilationUnit(CancellationToken.None)).Root;
        }

        public static SyntaxTree ParseExpression(string text)
        {
            return Parse(SourceText.From(text), null, null, p => p.ParseExpression());
        }

        public static StatementSyntax ParseStatement(string text)
        {
            return (StatementSyntax) Parse(SourceText.From(text), null, null, p => p.ParseStatement()).Root;
        }

        private static SyntaxTree Parse(SourceText sourceText, ParserOptions options, IIncludeFileSystem fileSystem, Func<HlslParser, SyntaxNode> parseFunc)
        {
            var lexer = new HlslLexer(sourceText, options, fileSystem);
            var parser = new HlslParser(lexer);

            var result = new SyntaxTree(sourceText,
                syntaxTree => new Tuple<SyntaxNode, List<FileSegment>>(
                    parseFunc(parser),
                    lexer.FileSegments));

            Debug.WriteLine(DateTime.Now +  " - Finished parsing");

            return result;
        }

        public static SyntaxToken ParseToken(string text)
        {
            return new HlslLexer(SourceText.From(text)).Lex(LexerMode.Syntax);
        }

        public static IReadOnlyList<SyntaxToken> ParseAllTokens(SourceText sourceText, IIncludeFileSystem fileSystem = null)
        {
            var tokens = new List<SyntaxToken>();

            var lexer = new HlslLexer(sourceText, fileSystem: fileSystem);
            SyntaxToken token;
            do
            {
                tokens.Add(token = lexer.Lex(LexerMode.Syntax));
            } while (token.Kind != SyntaxKind.EndOfFileToken);

            return tokens;
        }
    }
}