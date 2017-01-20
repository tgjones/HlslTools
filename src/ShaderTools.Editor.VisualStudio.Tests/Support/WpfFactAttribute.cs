using System;
using Xunit;
using Xunit.Sdk;

namespace ShaderTools.Editor.VisualStudio.Tests.Support
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("ShaderTools.Editor.VisualStudio.Tests.Support.WpfFactDiscoverer", "ShaderTools.Editor.VisualStudio.Tests")]
    public class WpfFactAttribute : FactAttribute
    {
    }
}
