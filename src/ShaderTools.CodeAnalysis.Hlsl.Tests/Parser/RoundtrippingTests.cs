using System.IO;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Hlsl.Tests.Support;
using ShaderTools.CodeAnalysis.Hlsl.Tests.TestSuite;
using ShaderTools.Testing;
using Xunit;

namespace ShaderTools.CodeAnalysis.Hlsl.Tests.Parser
{
    public class RoundtrippingTests
    {
        [Theory]
        [HlslTestSuiteData]
        public void CanBuildSyntaxTree(string testFile)
        {
            var options = new HlslParseOptions();
            if (testFile.StartsWith("TestSuite\\Shaders\\Nvidia"))
                options.AdditionalIncludeDirectories.Add("TestSuite\\Shaders\\Nvidia");

            var sourceCode = File.ReadAllText(testFile);

            // Build syntax tree.
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(new CodeAnalysis.Text.SourceFile(SourceText.From(sourceCode), testFile), options, fileSystem: new TestFileSystem());
            SyntaxTreeUtility.CheckForParseErrors(syntaxTree);

            // Check roundtripping.
            var roundtrippedText = syntaxTree.Root.ToFullString();
            Assert.Equal(sourceCode, roundtrippedText);
        }
    }
}