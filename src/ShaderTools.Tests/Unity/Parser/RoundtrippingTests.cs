using System.IO;
using ShaderTools.Core.Text;
using ShaderTools.Tests.Unity.Support;
using ShaderTools.Unity.Syntax;
using Xunit;

namespace ShaderTools.Tests.Unity.Parser
{
    public class RoundtrippingTests
    {
        [Theory]
        [MemberData(nameof(ShaderTestUtility.GetUnityTestShaders), MemberType = typeof(ShaderTestUtility))]
        public void CanBuildUnitySyntaxTree(string testFile)
        {
            var sourceCode = File.ReadAllText(testFile);

            // Build syntax tree.
            var syntaxTree = SyntaxFactory.ParseUnitySyntaxTree(
                SourceText.From(sourceCode));

            ShaderTestUtility.CheckForParseErrors(syntaxTree);

            // Check roundtripping.
            var roundtrippedText = syntaxTree.Root.ToFullString();
            Assert.Equal(sourceCode, roundtrippedText);
        }
    }
}