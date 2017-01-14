using System.IO;
using NUnit.Framework;
using ShaderTools.Core.Text;
using ShaderTools.Tests.Unity.Support;
using ShaderTools.Unity.Syntax;

namespace ShaderTools.Tests.Unity.Parser
{
    [TestFixture]
    public class RoundtrippingTests
    {
        [TestCaseSource(typeof(ShaderTestUtility), nameof(ShaderTestUtility.GetUnityTestShaders))]
        public void CanBuildUnitySyntaxTree(string testFile)
        {
            var sourceCode = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, testFile));

            // Build syntax tree.
            var syntaxTree = SyntaxFactory.ParseUnitySyntaxTree(
                SourceText.From(sourceCode));

            ShaderTestUtility.CheckForParseErrors(syntaxTree);

            // Check roundtripping.
            var roundtrippedText = syntaxTree.Root.ToFullString();
            Assert.That(roundtrippedText, Is.EqualTo(sourceCode));
        }
    }
}