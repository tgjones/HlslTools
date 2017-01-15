using System.Collections.Generic;
using ShaderTools.Tests.Hlsl.Support;

namespace ShaderTools.Editor.VisualStudio.Tests.Hlsl.Support
{
    internal static class VsShaderTestUtility
    {
        public static IEnumerable<object> GetTestShaders()
        {
            return ShaderTestUtility.FindTestShaders(@"Hlsl\Shaders");
        }
    }
}