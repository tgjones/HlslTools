using System.IO;
using HlslTools.Formatting;
using HlslTools.Syntax;
using HlslTools.Tests.Support;
using HlslTools.Text;
using NUnit.Framework;

namespace HlslTools.Tests.Formatting
{
    [TestFixture]
    public class IdempotentTests
    {
        [TestCaseSource(typeof(ShaderTestUtility), nameof(ShaderTestUtility.GetTestShaders))]
        public void CanFormatAndReformat(string testFile)
        {
            // Arrange.
            var sourceCode = File.ReadAllText(testFile);
            var syntaxTree1 = SyntaxFactory.ParseSyntaxTree(SourceText.From(sourceCode), fileSystem: new TestFileSystem(testFile));

            // Format.
            var formattedCode1 = FormatCode(sourceCode, syntaxTree1);
            var syntaxTree2 = SyntaxFactory.ParseSyntaxTree(SourceText.From(formattedCode1), fileSystem: new TestFileSystem(testFile));
            ShaderTestUtility.CheckForParseErrors(syntaxTree2);

            // Format again.
            var formattedCode2 = FormatCode(formattedCode1, syntaxTree2);
            var syntaxTree3 = SyntaxFactory.ParseSyntaxTree(SourceText.From(formattedCode2), fileSystem: new TestFileSystem(testFile));
            ShaderTestUtility.CheckForParseErrors(syntaxTree3);

            Assert.That(formattedCode2, Is.EqualTo(formattedCode1));
        }

        private static string FormatCode(string sourceCode, SyntaxTree syntaxTree)
        {
            var edits = Formatter.GetEdits(syntaxTree, new TextSpan(null, 0, sourceCode.Length), new FormattingOptions());
            return Formatter.ApplyEdits(sourceCode, edits);
        }
    }
}