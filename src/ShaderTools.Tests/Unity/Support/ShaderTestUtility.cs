using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ShaderTools.Unity.Syntax;

namespace ShaderTools.Tests.Unity.Support
{
    public static class ShaderTestUtility
    {
        public static IEnumerable<TestCaseData> FindTestShaders(string rootFolder)
        {
            return Directory.GetFiles(rootFolder, "*.*", SearchOption.AllDirectories)
                .Where(x =>
                {
                    var ext = Path.GetExtension(x).ToLower();
                    return ext == ".shader";
                })
                .Select(x => new TestCaseData(x));
        }

        internal static IEnumerable<TestCaseData> GetUnityTestShaders()
        {
            return FindTestShaders(@"Unity\Shaders");
        }

        public static void CheckForParseErrors(SyntaxTree syntaxTree)
        {
            foreach (var diagnostic in syntaxTree.GetDiagnostics())
                Debug.WriteLine(diagnostic.ToString());
            Assert.That(syntaxTree.GetDiagnostics().Count(), Is.EqualTo(0));
        }
    }
}