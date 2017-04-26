using System;
using System.Collections.Generic;
using ShaderTools.Testing;

namespace ShaderTools.CodeAnalysis.Hlsl.Tests.TestSuite
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class HlslTestSuiteDataAttribute : TestSuiteDataAttribute
    {
        protected override IEnumerable<string> FileExtensions { get; } = new[] { ".hlsl", ".fx", ".vsh", ".psh" };
    }
}