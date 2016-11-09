using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using HlslTools.Syntax;
using HlslTools.Tests.Support;
using HlslTools.Text;
using NUnit.Framework;

namespace HlslTools.Tests.Parser
{
    [TestFixture]
    public class HlslLexerTests
    {
        [Test]
        public void HandlesWhitespaceTrivia()
        {
            // Act.
            var allTokens = LexAllTokens(new StringText("a b \n d  "));

            // Assert.
            Assert.That(allTokens, Has.Count.EqualTo(4));

            Assert.That(allTokens[0].SourceRange, Is.EqualTo(new SourceRange(new SourceLocation(0), 1)));
            Assert.That(allTokens[0].FullSourceRange, Is.EqualTo(new SourceRange(new SourceLocation(0), 2)));
            Assert.That(allTokens[0].LeadingTrivia, Has.Length.EqualTo(0));
            Assert.That(allTokens[0].TrailingTrivia, Has.Length.EqualTo(1));
            Assert.That(allTokens[0].TrailingTrivia[0].SourceRange, Is.EqualTo(new SourceRange(new SourceLocation(1), 1)));
            Assert.That(allTokens[0].TrailingTrivia[0].Kind, Is.EqualTo(SyntaxKind.WhitespaceTrivia));
            Assert.That(((SyntaxTrivia)allTokens[0].TrailingTrivia[0]).Text, Is.EqualTo(" "));

            Assert.That(allTokens[1].SourceRange, Is.EqualTo(new SourceRange(new SourceLocation(2), 1)));
            Assert.That(allTokens[1].FullSourceRange, Is.EqualTo(new SourceRange(new SourceLocation(2), 3)));
            Assert.That(allTokens[1].LeadingTrivia, Has.Length.EqualTo(0));
            Assert.That(allTokens[1].TrailingTrivia, Has.Length.EqualTo(2));
            Assert.That(allTokens[1].TrailingTrivia[0].SourceRange, Is.EqualTo(new SourceRange(new SourceLocation(3), 1)));
            Assert.That(allTokens[1].TrailingTrivia[0].Kind, Is.EqualTo(SyntaxKind.WhitespaceTrivia));
            Assert.That(((SyntaxTrivia)allTokens[0].TrailingTrivia[0]).Text, Is.EqualTo(" "));
            Assert.That(allTokens[1].TrailingTrivia[1].SourceRange, Is.EqualTo(new SourceRange(new SourceLocation(4), 1)));
            Assert.That(allTokens[1].TrailingTrivia[1].Kind, Is.EqualTo(SyntaxKind.EndOfLineTrivia));
            Assert.That(((SyntaxTrivia)allTokens[1].TrailingTrivia[1]).Text, Is.EqualTo("\n"));

            Assert.That(allTokens[2].SourceRange, Is.EqualTo(new SourceRange(new SourceLocation(6), 1)));
            Assert.That(allTokens[2].FullSourceRange, Is.EqualTo(new SourceRange(new SourceLocation(5), 4)));
            Assert.That(allTokens[2].LeadingTrivia, Has.Length.EqualTo(1));
            Assert.That(allTokens[2].LeadingTrivia[0].SourceRange, Is.EqualTo(new SourceRange(new SourceLocation(5), 1)));
            Assert.That(allTokens[2].LeadingTrivia[0].Kind, Is.EqualTo(SyntaxKind.WhitespaceTrivia));
            Assert.That(((SyntaxTrivia)allTokens[2].LeadingTrivia[0]).Text, Is.EqualTo(" "));
            Assert.That(allTokens[2].TrailingTrivia, Has.Length.EqualTo(1));
            Assert.That(allTokens[2].TrailingTrivia[0].SourceRange, Is.EqualTo(new SourceRange(new SourceLocation(7), 2)));
            Assert.That(allTokens[2].TrailingTrivia[0].Kind, Is.EqualTo(SyntaxKind.WhitespaceTrivia));
            Assert.That(((SyntaxTrivia)allTokens[2].TrailingTrivia[0]).Text, Is.EqualTo("  "));

            Assert.That(allTokens[3].Kind, Is.EqualTo(SyntaxKind.EndOfFileToken));
            Assert.That(allTokens[3].SourceRange, Is.EqualTo(new SourceRange(new SourceLocation(9), 0)));
        }

        [TestCase("42", 42)]     // Decimal literal
        [TestCase("052", 42)]    // Octal literal
        [TestCase("0x2a", 0x2a)] // Hex literal
        [TestCase("0X2A", 0x2A)] // Hex literal
        [TestCase("42u", 42)]
        [TestCase("42U", 42)]
        [TestCase("42l", 42)]
        [TestCase("42L", 42)]
        [TestCase("42ul", 42)]
        [TestCase("42UL", 42)]
        [TestCase("42lu", 42)]
        public void TestIntegerLiterals(string text, int value)
        {
            var token = LexToken(text);

            Assert.NotNull(token);
            Assert.AreEqual(SyntaxKind.IntegerLiteralToken, token.Kind);
            var errors = token.GetDiagnostics().ToArray();
            Assert.AreEqual(0, errors.Length);
            Assert.AreEqual(text, token.Text);
            Assert.AreEqual(value, token.Value);
        }

        [TestCase("1.0", 1.0f)]
        [TestCase("1.0f", 1.0f)]
        [TestCase("1.0h", 1.0f)]
        public void TestFloatLiterals(string text, float value)
        {
            var token = LexToken(text);

            Assert.NotNull(token);
            Assert.AreEqual(SyntaxKind.FloatLiteralToken, token.Kind);
            var errors = token.GetDiagnostics().ToArray();
            Assert.AreEqual(0, errors.Length);
            Assert.AreEqual(text, token.Text);
            Assert.AreEqual(value, token.Value);
        }

        [TestCaseSource(typeof(ShaderTestUtility), nameof(ShaderTestUtility.GetTestShaders))]
        public void CanLexShader(string testFile)
        {
            // Act.
            var tokens = LexAllTokens(testFile);

            // Assert.
            Assert.That(tokens, Has.Count.GreaterThan(0));

            foreach (var token in tokens)
                foreach (var diagnostic in token.GetDiagnostics())
                    Debug.WriteLine($"{diagnostic} at {diagnostic.Span}");

            Assert.That(tokens.Any(t => t.ContainsDiagnostics), Is.False);
        }

        #region Test helpers

        private static IReadOnlyList<SyntaxToken> LexAllTokens(string testFile)
        {
            return LexAllTokens(SourceText.From(File.ReadAllText(testFile)), new TestFileSystem(testFile));
        }

        private static IReadOnlyList<SyntaxToken> LexAllTokens(SourceText text, IIncludeFileSystem fileSystem = null)
        {
            return SyntaxFactory.ParseAllTokens(text, fileSystem);
        }

        private static SyntaxToken LexToken(string text)
        {
            return SyntaxFactory.ParseToken(text);
        }

        #endregion
    }
}