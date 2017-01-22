using System;
using System.Collections.Generic;

namespace ShaderTools.Testing.TestResources.ShaderLab
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class ShaderLabTestSuiteDataAttribute : TestSuiteDataAttribute
    {
        protected override string DirectoryName { get; } = "ShaderLab";

        protected override IEnumerable<string> FileExtensions { get; } = new[] { ".shader" };
    }
}