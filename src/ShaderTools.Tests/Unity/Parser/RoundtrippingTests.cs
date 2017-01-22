using System.IO;
using ShaderTools.Core.Text;
using ShaderTools.Testing;
using ShaderTools.Testing.TestResources.ShaderLab;
using ShaderTools.Unity.Syntax;
using Xunit;

namespace ShaderTools.Tests.Unity.Parser
{
    public class RoundtrippingTests
    {
        [Theory]
        [ShaderLabTestSuiteData]
        public void CanBuildUnitySyntaxTree(string testFile)
        {
            var sourceCode = File.ReadAllText(testFile);

            // Build syntax tree.
            var syntaxTree = SyntaxFactory.ParseUnitySyntaxTree(
                SourceText.From(sourceCode));

            SyntaxTreeUtility.CheckForParseErrors(syntaxTree);

            // Check roundtripping.
            var roundtrippedText = syntaxTree.Root.ToFullString();
            Assert.Equal(sourceCode, roundtrippedText);
        }
    }
}