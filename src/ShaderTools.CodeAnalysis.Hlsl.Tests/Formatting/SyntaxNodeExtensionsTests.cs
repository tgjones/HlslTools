using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Hlsl.Formatting;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Hlsl.Tests.Support;
using ShaderTools.CodeAnalysis.Hlsl.Tests.TestSuite;
using ShaderTools.CodeAnalysis.Text;
using Xunit;

namespace ShaderTools.CodeAnalysis.Hlsl.Tests.Formatting
{
    public class SyntaxNodeExtensionsTests
    {
        [Theory]
        [HlslTestSuiteData]
        public void CanGetRootLocatedNodes(string testFile)
        {
            var sourceCode = File.ReadAllText(testFile);

            // Build syntax tree.
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(
                new SourceFile(SourceText.From(sourceCode), testFile), 
                fileSystem: new TestFileSystem());

            // Check roundtripping.
            var allRootTokensAndTrivia = ((SyntaxNode) syntaxTree.Root).GetRootLocatedNodes();
            var sb = new StringBuilder();
            foreach (var locatedNode in allRootTokensAndTrivia)
                sb.Append(locatedNode.Text);
            var roundtrippedText = sb.ToString();
            Assert.Equal(sourceCode, roundtrippedText);
        }
    }
}