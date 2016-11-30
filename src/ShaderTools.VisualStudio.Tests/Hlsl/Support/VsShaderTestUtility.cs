using System.Collections.Generic;
using NUnit.Framework;
using ShaderTools.Tests.Hlsl.Support;

namespace ShaderTools.VisualStudio.Tests.Hlsl.Support
{
    internal static class VsShaderTestUtility
    {
        public static IEnumerable<TestCaseData> GetTestShaders()
        {
            return ShaderTestUtility.FindTestShaders("../../../ShaderTools.Tests/Hlsl/Shaders");
        }
    }
}