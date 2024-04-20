using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Hlsl.Formatting;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Hlsl.Tests.Support;
using ShaderTools.CodeAnalysis.Hlsl.Tests.TestSuite;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Testing;
using Xunit;

namespace ShaderTools.CodeAnalysis.Hlsl.Tests.Formatting
{
    public class SyntaxNodeExtensionsTests
    {
        [Theory]
        [HlslTestSuiteData]
        public void CanGetRootLocatedNodes(string testFile)
        {
            var options = new HlslParseOptions();
            if (testFile.StartsWith("TestSuite\\Shaders\\Nvidia"))
                options.AdditionalIncludeDirectories.Add("TestSuite\\Shaders\\Nvidia");

            var sourceCode = File.ReadAllText(testFile);

            // Build syntax tree.
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(new SourceFile(SourceText.From(sourceCode), testFile), options, fileSystem: new TestFileSystem());
            SyntaxTreeUtility.CheckForParseErrors(syntaxTree);

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