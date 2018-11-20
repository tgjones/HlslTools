using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ShaderTools.CodeAnalysis.Hlsl.Parser;
using ShaderTools.CodeAnalysis.Hlsl.Text;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public static class SyntaxFactory
    {
        public static SyntaxTree ParseSyntaxTree(SourceText sourceText, HlslParseOptions options = null, IIncludeFileSystem fileSystem = null, IIncludeFileResolver fileResolver = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Parse(sourceText, options, fileSystem ?? new DummyFileSystem(), fileResolver, p => p.ParseCompilationUnit(cancellationToken));
        }

        public static CompilationUnitSyntax ParseCompilationUnit(SourceText sourceText, IIncludeFileSystem fileSystem = null, IIncludeFileResolver fileResolver = null)
        {
            return (CompilationUnitSyntax) Parse(sourceText, null, fileSystem, fileResolver, p => p.ParseCompilationUnit(CancellationToken.None)).Root;
        }

        public static SyntaxTree ParseExpression(string text)
        {
            return Parse(SourceText.From(text), null, null, null, p => p.ParseExpression());
        }

        public static StatementSyntax ParseStatement(string text)
        {
            return (StatementSyntax) Parse(SourceText.From(text), null, null, null, p => p.ParseStatement()).Root;
        }

        private static SyntaxTree Parse(SourceText sourceText, HlslParseOptions options, IIncludeFileSystem fileSystem, IIncludeFileResolver fileResolver, Func <HlslParser, SyntaxNode> parseFunc)
        {
            var sourceFile = new SourceFile(sourceText, null);

            var lexer = new HlslLexer(sourceFile, options, fileSystem, fileResolver);
            var parser = new HlslParser(lexer);

            var result = new SyntaxTree(
                sourceFile,
                options,
                syntaxTree =>
                {
                    var node = parseFunc(parser);
                    node.SetSyntaxTree(syntaxTree);

                    return new Tuple<SyntaxNode, List<FileSegment>>(
                        node,
                        lexer.FileSegments);
                });

            Debug.WriteLine(DateTime.Now +  " - Finished parsing");

            return result;
        }

        public static SyntaxToken ParseToken(string text)
        {
            return new HlslLexer(new SourceFile(SourceText.From(text), null)).Lex(LexerMode.Syntax);
        }

        public static IReadOnlyList<SyntaxToken> ParseAllTokens(SourceText sourceText, IIncludeFileSystem fileSystem = null)
        {
            var tokens = new List<SyntaxToken>();

            var lexer = new HlslLexer(new SourceFile(sourceText, null), includeFileSystem: fileSystem);
            SyntaxToken token;
            do
            {
                tokens.Add(token = lexer.Lex(LexerMode.Syntax));
            } while (token.Kind != SyntaxKind.EndOfFileToken);

            return tokens;
        }
    }
}