using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ShaderTools.Unity.Syntax;
using Xunit;

namespace ShaderTools.Tests.Unity.Support
{
    public static class ShaderTestUtility
    {
        private static IEnumerable<object> FindTestShaders(string rootFolder)
        {
            return Directory.GetFiles(rootFolder, "*.*", SearchOption.AllDirectories)
                .Where(x =>
                {
                    var ext = Path.GetExtension(x).ToLower();
                    return ext == ".shader";
                })
                .Select(x => new object[] { x });
        }

        internal static IEnumerable<object> GetUnityTestShaders()
        {
            return FindTestShaders(@"Unity\Shaders");
        }

        public static void CheckForParseErrors(SyntaxTree syntaxTree)
        {
            foreach (var diagnostic in syntaxTree.GetDiagnostics())
                Debug.WriteLine(diagnostic.ToString());
            Assert.Empty(syntaxTree.GetDiagnostics());
        }
    }
}