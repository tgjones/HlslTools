using System;
using System.Collections.Generic;
using ShaderTools.Testing;

namespace ShaderTools.CodeAnalysis.ShaderLab.Tests.TestSuite
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class ShaderLabTestSuiteDataAttribute : TestSuiteDataAttribute
    {
        protected override IEnumerable<string> FileExtensions { get; } = new[] { ".shader" };
    }
}