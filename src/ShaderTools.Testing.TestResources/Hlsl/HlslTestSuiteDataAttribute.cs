using System;
using System.Collections.Generic;

namespace ShaderTools.Testing.TestResources.Hlsl
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class HlslTestSuiteDataAttribute : TestSuiteDataAttribute
    {
        protected override string DirectoryName { get; } = "Hlsl";

        protected override IEnumerable<string> FileExtensions { get; } = new[] { ".hlsl", ".fx", ".vsh", ".psh" };
    }
}