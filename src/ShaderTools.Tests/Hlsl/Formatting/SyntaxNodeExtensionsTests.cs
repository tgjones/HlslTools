using System.IO;
using System.Text;
using NUnit.Framework;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Formatting;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.Tests.Hlsl.Support;

namespace ShaderTools.Tests.Hlsl.Formatting
{
    [TestFixture]
    public class SyntaxNodeExtensionsTests
    {
        [TestCaseSource(typeof(ShaderTestUtility), nameof(ShaderTestUtility.GetTestShaders))]
        public void CanGetRootLocatedNodes(string testFile)
        {
            testFile = Path.Combine(TestContext.CurrentContext.TestDirectory, testFile);

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
            Assert.That(roundtrippedText, Is.EqualTo(sourceCode));
        }
    }
}