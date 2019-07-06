using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Hlsl.Parser;
using ShaderTools.CodeAnalysis.Hlsl.Text;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public static class SyntaxFactory
    {
        public static SyntaxTree ParseSyntaxTree(SourceFile file, HlslParseOptions options = null, IIncludeFileSystem fileSystem = null, CancellationToken cancellationToken = default)
        {
            return Parse(file, options, fileSystem ?? new DummyFileSystem(), p => p.ParseCompilationUnit(cancellationToken));
        }

        internal static SyntaxTree ParseSyntaxTree(SourceText sourceText, HlslParseOptions options = null, IIncludeFileSystem fileSystem = null, CancellationToken cancellationToken = default)
        {
            return ParseSyntaxTree(new SourceFile(sourceText), options, fileSystem, cancellationToken);
        }

        public static CompilationUnitSyntax ParseCompilationUnit(SourceFile file, IIncludeFileSystem fileSystem = null)
        {
            return (CompilationUnitSyntax) Parse(file, null, fileSystem, p => p.ParseCompilationUnit(CancellationToken.None)).Root;
        }

        public static SyntaxTree ParseExpression(string text)
        {
            return Parse(new SourceFile(SourceText.From(text)), null, null, p => p.ParseExpression());
        }

        public static StatementSyntax ParseStatement(string text)
        {
            return (StatementSyntax) Parse(new SourceFile(SourceText.From(text)), null, null, p => p.ParseStatement()).Root;
        }

        private static SyntaxTree Parse(SourceFile sourceFile, HlslParseOptions options, IIncludeFileSystem fileSystem, Func<HlslParser, SyntaxNode> parseFunc)
        {
            var lexer = new HlslLexer(sourceFile, options, fileSystem);
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
            return new HlslLexer(new SourceFile(SourceText.From(text))).Lex(LexerMode.Syntax);
        }

        public static IReadOnlyList<SyntaxToken> ParseAllTokens(SourceFile file, IIncludeFileSystem fileSystem = null)
        {
            var tokens = new List<SyntaxToken>();

            var lexer = new HlslLexer(file, includeFileSystem: fileSystem);
            SyntaxToken token;
            do
            {
                tokens.Add(token = lexer.Lex(LexerMode.Syntax));
            } while (token.Kind != SyntaxKind.EndOfFileToken);

            return tokens;
        }
    }
}