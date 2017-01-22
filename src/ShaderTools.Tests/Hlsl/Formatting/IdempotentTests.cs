using System.IO;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Formatting;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.Testing;
using ShaderTools.Testing.TestResources.Hlsl;
using ShaderTools.Tests.Hlsl.Support;
using Xunit;

namespace ShaderTools.Tests.Hlsl.Formatting
{
    public class IdempotentTests
    {
        [Theory]
        [HlslTestSuiteData]
        public void CanFormatAndReformat(string testFile)
        {
            // Arrange.
            var sourceCode = File.ReadAllText(testFile);
            var syntaxTree1 = SyntaxFactory.ParseSyntaxTree(SourceText.From(sourceCode, testFile), fileSystem: new TestFileSystem());

            // Format.
            var formattedCode1 = FormatCode(sourceCode, syntaxTree1);
            var syntaxTree2 = SyntaxFactory.ParseSyntaxTree(SourceText.From(formattedCode1, testFile), fileSystem: new TestFileSystem());
            SyntaxTreeUtility.CheckForParseErrors(syntaxTree2);

            // Format again.
            var formattedCode2 = FormatCode(formattedCode1, syntaxTree2);
            var syntaxTree3 = SyntaxFactory.ParseSyntaxTree(SourceText.From(formattedCode2, testFile), fileSystem: new TestFileSystem());
            SyntaxTreeUtility.CheckForParseErrors(syntaxTree3);

            Assert.Equal(formattedCode1, formattedCode2);
        }

        private static string FormatCode(string sourceCode, SyntaxTree syntaxTree)
        {
            var edits = Formatter.GetEdits(syntaxTree, new TextSpan(syntaxTree.Text, 0, sourceCode.Length), new FormattingOptions());
            return Formatter.ApplyEdits(sourceCode, edits);
        }
    }
}