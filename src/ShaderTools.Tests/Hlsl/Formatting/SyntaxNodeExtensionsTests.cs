using System.IO;
using System.Text;
using NUnit.Framework;
using ShaderTools.Hlsl.Formatting;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.Hlsl.Text;
using ShaderTools.Tests.Hlsl.Support;

namespace ShaderTools.Tests.Hlsl.Formatting
{
    [TestFixture]
    public class SyntaxNodeExtensionsTests
    {
        [TestCaseSource(typeof(ShaderTestUtility), nameof(ShaderTestUtility.GetTestShaders))]
        public void CanGetRootLocatedNodes(string testFile)
        {
            var sourceCode = File.ReadAllText(testFile);

            // Build syntax tree.
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(sourceCode), fileSystem: new TestFileSystem(testFile));

            // Check roundtripping.
            var allRootTokensAndTrivia = syntaxTree.Root.GetRootLocatedNodes();
            var sb = new StringBuilder();
            foreach (var locatedNode in allRootTokensAndTrivia)
                sb.Append(locatedNode.Text);
            var roundtrippedText = sb.ToString();
            Assert.That(roundtrippedText, Is.EqualTo(sourceCode));
        }
    }
}