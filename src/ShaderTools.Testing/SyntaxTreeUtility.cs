using System.Diagnostics;
using ShaderTools.Core.Syntax;
using Xunit;

namespace ShaderTools.Testing
{
    public static class SyntaxTreeUtility
    {
        public static void CheckForParseErrors(SyntaxTreeBase syntaxTree)
        {
            foreach (var diagnostic in syntaxTree.GetDiagnostics())
                Debug.WriteLine(diagnostic.ToString());
            Assert.Empty(syntaxTree.GetDiagnostics());
        }
    }
}
