using System.IO;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.ShaderLab.Syntax;
using ShaderTools.CodeAnalysis.ShaderLab.Tests.TestSuite;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Testing;
using Xunit;

namespace ShaderTools.CodeAnalysis.ShaderLab.Tests.Parser
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