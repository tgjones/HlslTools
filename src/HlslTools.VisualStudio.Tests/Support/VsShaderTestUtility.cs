using System.Collections.Generic;
using NUnit.Framework;

namespace HlslTools.VisualStudio.Tests.Support
{
    internal static class VsShaderTestUtility
    {
        public static IEnumerable<TestCaseData> GetTestShaders()
        {
            return HlslTools.Tests.Support.ShaderTestUtility.FindTestShaders("../../../HlslTools.Tests/Shaders");
        }
    }
}