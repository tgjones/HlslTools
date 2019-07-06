using System.IO;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Hlsl.Formatting;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Hlsl.Tests.Support;
using ShaderTools.CodeAnalysis.Hlsl.Tests.TestSuite;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Testing;
using Xunit;

namespace ShaderTools.CodeAnalysis.Hlsl.Tests.Formatting
{
    public class IdempotentTests
    {
        [Theory]
        [HlslTestSuiteData]
        public void CanFormatAndReformat(string testFile)
        {
            // Arrange.
            var sourceCode = File.ReadAllText(testFile);
            var syntaxTree1 = SyntaxFactory.ParseSyntaxTree(new SourceFile(SourceText.From(sourceCode), testFile), fileSystem: new TestFileSystem());

            // Format.
            var formattedCode1 = FormatCode(sourceCode, syntaxTree1);
            var syntaxTree2 = SyntaxFactory.ParseSyntaxTree(new SourceFile(SourceText.From(formattedCode1), testFile), fileSystem: new TestFileSystem());
            SyntaxTreeUtility.CheckForParseErrors(syntaxTree2);

            // Format again.
            var formattedCode2 = FormatCode(formattedCode1, syntaxTree2);
            var syntaxTree3 = SyntaxFactory.ParseSyntaxTree(new SourceFile(SourceText.From(formattedCode2), testFile), fileSystem: new TestFileSystem());
            SyntaxTreeUtility.CheckForParseErrors(syntaxTree3);

            Assert.Equal(formattedCode1, formattedCode2);
        }

        private static string FormatCode(string sourceCode, SyntaxTree syntaxTree)
        {
            var edits = Formatter.GetEdits(syntaxTree, (SyntaxNode) syntaxTree.Root, new TextSpan(0, sourceCode.Length), new FormattingOptions());
            return Formatter.ApplyEdits(sourceCode, edits);
        }
    }
}