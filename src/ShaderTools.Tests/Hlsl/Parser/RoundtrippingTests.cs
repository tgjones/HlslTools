using System.IO;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.Testing;
using ShaderTools.Testing.TestResources.Hlsl;
using ShaderTools.Tests.Hlsl.Support;
using Xunit;

namespace ShaderTools.Tests.Hlsl.Parser
{
    public class RoundtrippingTests
    {
        [Theory]
        [HlslTestSuiteData]
        public void CanBuildSyntaxTree(string testFile)
        {
            var sourceCode = File.ReadAllText(testFile);

            // Build syntax tree.
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(sourceCode, testFile), fileSystem: new TestFileSystem());

            SyntaxTreeUtility.CheckForParseErrors(syntaxTree);

            // Check roundtripping.
            var roundtrippedText = syntaxTree.Root.ToFullString();
            Assert.Equal(sourceCode, roundtrippedText);
        }
    }
}