using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ShaderTools.Core.Text;
using ShaderTools.Tests.Unity.Support;
using ShaderTools.Unity.Syntax;

namespace ShaderTools.Tests.Unity.Parser
{
    [TestFixture]
    public class UnityLexerTests
    {
        [TestCaseSource(typeof(ShaderTestUtility), nameof(ShaderTestUtility.GetUnityTestShaders))]
        public void CanLexUnityShader(string testFile)
        {
            // Unity includes some headers by default. And built-in headers are available without a path prefix.
            // http://docs.unity3d.com/Manual/SL-BuiltinIncludes.html

            // Act.
            var tokens = LexAllTokens(SourceText.From(File.ReadAllText(testFile)));

            // Assert.
            Assert.That(tokens, Has.Count.GreaterThan(0));

            foreach (var token in tokens)
                foreach (var diagnostic in token.GetDiagnostics())
                    Debug.WriteLine($"{diagnostic} at {diagnostic.SourceRange}");

            Assert.That(tokens.Any(t => t.ContainsDiagnostics), Is.False);
        }

        #region Test helpers

        private static IReadOnlyList<SyntaxToken> LexAllTokens(SourceText text)
        {
            return SyntaxFactory.ParseAllTokens(text);
        }

        #endregion
    }
}