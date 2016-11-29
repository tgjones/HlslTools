using System.IO;
using NUnit.Framework;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.Hlsl.Text;
using ShaderTools.Tests.Hlsl.Support;

namespace ShaderTools.Tests.Hlsl.Parser
{
    [TestFixture]
    public class RoundtrippingTests
    {
        [TestCaseSource(typeof(ShaderTestUtility), nameof(ShaderTestUtility.GetTestShaders))]
        public void CanBuildSyntaxTree(string testFile)
        {
            var sourceCode = File.ReadAllText(testFile);

            // Build syntax tree.
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(sourceCode), fileSystem: new TestFileSystem(testFile));

            ShaderTestUtility.CheckForParseErrors(syntaxTree);

            // Check roundtripping.
            var roundtrippedText = syntaxTree.Root.ToFullString();
            Assert.That(roundtrippedText, Is.EqualTo(sourceCode));
        }
    }
}