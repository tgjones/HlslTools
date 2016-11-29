using System.IO;
using NUnit.Framework;
using ShaderTools.Hlsl.Formatting;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.Hlsl.Text;
using ShaderTools.Tests.Hlsl.Support;

namespace ShaderTools.Tests.Hlsl.Formatting
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
            var edits = Formatter.GetEdits(syntaxTree, new TextSpan(syntaxTree.Text, 0, sourceCode.Length), new FormattingOptions());
            return Formatter.ApplyEdits(sourceCode, edits);
        }
    }
}