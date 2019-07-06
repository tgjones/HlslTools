using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.ShaderLab.Syntax;
using ShaderTools.CodeAnalysis.ShaderLab.Tests.TestSuite;
using Xunit;

namespace ShaderTools.CodeAnalysis.ShaderLab.Tests.Parser
{
    public class UnityLexerTests
    {
        [Theory]
        [ShaderLabTestSuiteData]
        public void CanLexUnityShader(string testFile)
        {
            // Unity includes some headers by default. And built-in headers are available without a path prefix.
            // http://docs.unity3d.com/Manual/SL-BuiltinIncludes.html

            // Act.
            var tokens = LexAllTokens(SourceText.From(File.ReadAllText(testFile)));

            // Assert.
            Assert.NotEmpty(tokens);

            foreach (var token in tokens)
                foreach (var diagnostic in token.GetDiagnostics())
                    Debug.WriteLine($"{diagnostic} at {diagnostic.SourceRange}");

            Assert.False(tokens.Any(t => t.ContainsDiagnostics));
        }

        [Fact]
        public void CanIncrementallyLexShader()
        {
            const string originalShader = @"
Shader ""MyShader"" {
    Properties {
        _Color(""Main Color"", Color) = (1, 1, 1, 1)
    }
}";

            var originalTokens = LexAllTokens(SourceText.From(originalShader));

            // TODO: Store PretokenizedText as lazily-computed property of SourceText.
            // TODO: Add SourceText constructor that takes a previous PretokenizedText
            //       and a list of TextChange objects.
        }

        #region Test helpers

        private static IReadOnlyList<SyntaxToken> LexAllTokens(SourceText text)
        {
            return SyntaxFactory.ParseAllTokens(text);
        }

        #endregion
    }
}