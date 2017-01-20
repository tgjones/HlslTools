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
            var configFile = ConfigFileLoader.LoadAndMergeConfigFile(Path.GetFullPath(@"Core\Options\Assets\ChildFolder\GrandchildFolder"));

            Assert.Equal(
                new Dictionary<string, string>
                {
                    { "MY_DEFINE", "1" },
                    { "MY_OTHER_DEFINE", "2" }
                }, 
                configFile.HlslPreprocessorDefinitions);

            var workingDirectory = Path.GetFullPath(@"Core\Options\Assets");
            Assert.Equal(
                new string[]
                {
                    Path.Combine(workingDirectory, "ChildFolder", "Bar"),
                    Path.Combine(workingDirectory, "ChildFolder", "Foo"),
                    Path.Combine(workingDirectory, "ChildFolder"),
                    workingDirectory
                },
                configFile.HlslAdditionalIncludeDirectories);
        }

        [Fact]
        public void CanLoadConfigFileWithMissingSettings()
        {
            var configFile = ConfigFileLoader.LoadAndMergeConfigFile(Path.GetFullPath(@"Core\Options\Assets\Folder2"));

            Assert.Empty(configFile.HlslPreprocessorDefinitions);
            Assert.Empty(configFile.HlslAdditionalIncludeDirectories);
        }
    }
}