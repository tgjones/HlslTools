using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ShaderTools.Core.Text;
using ShaderTools.Tests.Unity.Support;
using ShaderTools.Unity.Syntax;
using Xunit;

namespace ShaderTools.Tests.Unity.Parser
{
    public class UnityLexerTests
    {
        [Theory]
        [MemberData(nameof(ShaderTestUtility.GetUnityTestShaders), MemberType = typeof(ShaderTestUtility))]
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

        #region Test helpers

        private static IReadOnlyList<SyntaxToken> LexAllTokens(SourceText text)
        {
            return SyntaxFactory.ParseAllTokens(text);
        }

        #endregion
    }
}