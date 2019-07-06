using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Hlsl.Tests.Support;
using ShaderTools.CodeAnalysis.Hlsl.Tests.TestSuite;
using ShaderTools.CodeAnalysis.Hlsl.Text;
using ShaderTools.CodeAnalysis.Text;
using Xunit;

namespace ShaderTools.CodeAnalysis.Hlsl.Tests.Parser
{
    public class HlslLexerTests
    {
        [Fact]
        public void HandlesWhitespaceTrivia()
        {
            // Act.
            var allTokens = LexAllTokens(new SourceFile(SourceText.From("a b \n d  ")));

            // Assert.
            Assert.Equal(4, allTokens.Count);

            Assert.Equal(new SourceRange(new SourceLocation(0), 1), allTokens[0].SourceRange);
            Assert.Equal(new SourceRange(new SourceLocation(0), 2), allTokens[0].FullSourceRange);
            Assert.Empty(allTokens[0].LeadingTrivia);
            Assert.Equal(1, allTokens[0].TrailingTrivia.Length);
            Assert.Equal(new SourceRange(new SourceLocation(1), 1), allTokens[0].TrailingTrivia[0].SourceRange);
            Assert.Equal(SyntaxKind.WhitespaceTrivia, allTokens[0].TrailingTrivia[0].Kind);
            Assert.Equal(" ", ((SyntaxTrivia)allTokens[0].TrailingTrivia[0]).Text);

            Assert.Equal(new SourceRange(new SourceLocation(2), 1), allTokens[1].SourceRange);
            Assert.Equal(new SourceRange(new SourceLocation(2), 3), allTokens[1].FullSourceRange);
            Assert.Empty(allTokens[1].LeadingTrivia);
            Assert.Equal(2, allTokens[1].TrailingTrivia.Length);
            Assert.Equal(new SourceRange(new SourceLocation(3), 1), allTokens[1].TrailingTrivia[0].SourceRange);
            Assert.Equal(SyntaxKind.WhitespaceTrivia, allTokens[1].TrailingTrivia[0].Kind);
            Assert.Equal(" ", ((SyntaxTrivia)allTokens[0].TrailingTrivia[0]).Text);
            Assert.Equal(new SourceRange(new SourceLocation(4), 1), allTokens[1].TrailingTrivia[1].SourceRange);
            Assert.Equal(SyntaxKind.EndOfLineTrivia, allTokens[1].TrailingTrivia[1].Kind);
            Assert.Equal("\n", ((SyntaxTrivia)allTokens[1].TrailingTrivia[1]).Text);

            Assert.Equal(new SourceRange(new SourceLocation(6), 1), allTokens[2].SourceRange);
            Assert.Equal(new SourceRange(new SourceLocation(5), 4), allTokens[2].FullSourceRange);
            Assert.Equal(1, allTokens[2].LeadingTrivia.Length);
            Assert.Equal(new SourceRange(new SourceLocation(5), 1), allTokens[2].LeadingTrivia[0].SourceRange);
            Assert.Equal(SyntaxKind.WhitespaceTrivia, allTokens[2].LeadingTrivia[0].Kind);
            Assert.Equal(" ", ((SyntaxTrivia)allTokens[2].LeadingTrivia[0]).Text);
            Assert.Equal(1, allTokens[2].TrailingTrivia.Length);
            Assert.Equal(new SourceRange(new SourceLocation(7), 2), allTokens[2].TrailingTrivia[0].SourceRange);
            Assert.Equal(SyntaxKind.WhitespaceTrivia, allTokens[2].TrailingTrivia[0].Kind);
            Assert.Equal("  ", ((SyntaxTrivia)allTokens[2].TrailingTrivia[0]).Text);

            Assert.Equal(SyntaxKind.EndOfFileToken, allTokens[3].Kind);
            Assert.Equal(new SourceRange(new SourceLocation(9), 0), allTokens[3].SourceRange);
        }

        [Theory]
        [InlineData("42", 42)]     // Decimal literal
        [InlineData("052", 42)]    // Octal literal
        [InlineData("0x2a", 0x2a)] // Hex literal
        [InlineData("0X2A", 0x2A)] // Hex literal
        [InlineData("42u", 42)]
        [InlineData("42U", 42)]
        [InlineData("42l", 42)]
        [InlineData("42L", 42)]
        [InlineData("42ul", 42)]
        [InlineData("42UL", 42)]
        [InlineData("42lu", 42)]
        public void TestIntegerLiterals(string text, int value)
        {
            var token = LexToken(text);

            Assert.NotNull(token);
            Assert.Equal(SyntaxKind.IntegerLiteralToken, token.Kind);
            var errors = token.GetDiagnostics().ToArray();
            Assert.Equal(0, errors.Length);
            Assert.Equal(text, token.Text);
            Assert.Equal(value, token.Value);
        }

        [Theory]
        [InlineData("1.0", 1.0)]
        [InlineData("1.0f", 1.0)]
        [InlineData("1.0h", 1.0)]
        [InlineData("1.#IND", float.NaN)]
        [InlineData("1.#INF", float.PositiveInfinity)]
        public void TestFloatLiterals(string text, object value)
        {
            var token = LexToken(text);

            Assert.NotNull(token);
            Assert.Equal(SyntaxKind.FloatLiteralToken, token.Kind);
            var errors = token.GetDiagnostics().ToArray();
            Assert.Equal(0, errors.Length);
            Assert.Equal(text, token.Text);
            Assert.Equal(value, token.Value);
        }

        [Theory]
        [HlslTestSuiteData]
        public void CanLexShader(string testFile)
        {
            // Act.
            var tokens = LexAllTokens(testFile);

            // Assert.
            Assert.NotEmpty(tokens);

            foreach (var token in tokens)
                foreach (var diagnostic in token.GetDiagnostics())
                    Debug.WriteLine($"{diagnostic} at {diagnostic.SourceRange}");

            Assert.False(tokens.Any(t => t.ContainsDiagnostics));
        }

        #region Test helpers

        private static IReadOnlyList<SyntaxToken> LexAllTokens(string testFile)
        {
            return LexAllTokens(new SourceFile(SourceText.From(File.ReadAllText(testFile)), testFile), new TestFileSystem());
        }

        private static IReadOnlyList<SyntaxToken> LexAllTokens(SourceFile file, IIncludeFileSystem fileSystem = null)
        {
            return SyntaxFactory.ParseAllTokens(file, fileSystem);
        }

        private static SyntaxToken LexToken(string text)
        {
            return SyntaxFactory.ParseToken(text);
        }

        #endregion
    }
}