using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ShaderTools.Hlsl.Syntax;
using Xunit;

namespace ShaderTools.Tests.Hlsl.Support
{
    public static class ShaderTestUtility
    {
        public static IEnumerable<object> FindTestShaders(string rootFolder)
        {
            return Directory.GetFiles(rootFolder, "*.*", SearchOption.AllDirectories)
                .Where(x =>
                {
                    var ext = Path.GetExtension(x).ToLower();
                    return ext == ".hlsl" || ext == ".fx" || ext == ".vsh" || ext == ".psh";
                })
                .Select(x => new object[] { x });
        }

        internal static IEnumerable<object> GetTestShaders()
        {
            return FindTestShaders(@"Hlsl\Shaders");
        }

        public static void CheckForParseErrors(SyntaxTree syntaxTree)
        {
            foreach (var diagnostic in syntaxTree.GetDiagnostics())
                Debug.WriteLine(diagnostic.ToString());
            Assert.Empty(syntaxTree.GetDiagnostics());
        }
    }
}