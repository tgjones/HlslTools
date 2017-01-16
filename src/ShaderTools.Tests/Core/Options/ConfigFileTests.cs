using System.Collections.Generic;
using System.IO;
using ShaderTools.Core.Options;
using Xunit;

namespace ShaderTools.Tests.Core.Options
{
    public class ConfigFileTests
    {
        [Fact]
        public void CanLoadConfigFile()
        {
            var configFile = ConfigFileLoader.LoadAndMergeConfigFile(@"ChildFolder\GrandchildFolder");

            Assert.Equal(
                new Dictionary<string, string>
                {
                    { "MY_DEFINE", "1" },
                    { "MY_OTHER_DEFINE", "2" }
                }, 
                configFile.HlslPreprocessorDefinitions);

            Assert.Equal(
                new string[]
                {
                    "../Bar",
                    "..\\Foo",
                    ".",
                    "..",
                    "Foo"
                },
                configFile.HlslAdditionalIncludeDirectories);

            var workingDirectory = Path.GetFullPath(".");
            Assert.Equal(
                new string[]
                {
                    Path.Combine(workingDirectory, "ChildFolder", "Bar"),
                    Path.Combine(workingDirectory, "ChildFolder", "Foo"),
                    Path.Combine(workingDirectory, "ChildFolder"),
                    workingDirectory,
                    Path.Combine(workingDirectory, "ChildFolder", "Foo"),
                },
                configFile.GetAbsoluteHlslAdditionalIncludeDirectories());
        }
    }
}