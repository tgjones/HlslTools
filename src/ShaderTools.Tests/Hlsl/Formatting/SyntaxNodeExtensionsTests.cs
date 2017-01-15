using System.IO;
using System.Text;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Formatting;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.Tests.Hlsl.Support;
using Xunit;

namespace ShaderTools.Tests.Hlsl.Formatting
{
    public class SyntaxNodeExtensionsTests
    {
        [Theory]
        [MemberData(nameof(ShaderTestUtility.GetTestShaders), MemberType = typeof(ShaderTestUtility))]
        public void CanGetRootLocatedNodes(string testFile)
        {
            var sourceCode = File.ReadAllText(testFile);

            // Build syntax tree.
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(
                SourceText.From(sourceCode, testFile), 
                fileSystem: new TestFileSystem());

            // Check roundtripping.
            var allRootTokensAndTrivia = syntaxTree.Root.GetRootLocatedNodes();
            var sb = new StringBuilder();
            foreach (var locatedNode in allRootTokensAndTrivia)
                sb.Append(locatedNode.Text);
            var roundtrippedText = sb.ToString();
            Assert.Equal(sourceCode, roundtrippedText);
        }
    }
}