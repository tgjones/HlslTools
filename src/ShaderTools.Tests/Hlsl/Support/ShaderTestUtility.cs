using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.Tests.Hlsl.Support
{
    public static class ShaderTestUtility
    {
        public static IEnumerable<TestCaseData> FindTestShaders(string rootFolder)
        {
            var testDirectory = TestContext.CurrentContext.TestDirectory;
            return Directory.GetFiles(Path.Combine(testDirectory, rootFolder), "*.*", SearchOption.AllDirectories)
                .Where(x =>
                {
                    var ext = Path.GetExtension(x).ToLower();
                    return ext == ".hlsl" || ext == ".fx" || ext == ".vsh" || ext == ".psh";
                })
                .Select(x => x.Substring(testDirectory.Length + 1))
                .Select(x => new TestCaseData(x)
                {
                    TestName = "{m}{a:1000}"
                });
        }

        internal static IEnumerable<TestCaseData> GetTestShaders()
        {
            return FindTestShaders(@"Hlsl\Shaders");
        }

        public static void CheckForParseErrors(SyntaxTree syntaxTree)
        {
            foreach (var diagnostic in syntaxTree.GetDiagnostics())
                Debug.WriteLine(diagnostic.ToString());
            Assert.That(syntaxTree.GetDiagnostics().Count(), Is.EqualTo(0));
        }
    }
}