using System.Collections.Generic;
using System.IO;
using ShaderTools.CodeAnalysis.Options;
using Xunit;

namespace ShaderTools.CodeAnalysis.Tests.Options
{
    public class ConfigFileTests
    {
        [Fact]
        public void CanLoadConfigFile()
        {
            var configFile = ConfigFileLoader.LoadAndMergeConfigFile(Path.GetFullPath(@"Options\Assets\ChildFolder\GrandchildFolder"));

            Assert.Equal(
                new Dictionary<string, string>
                {
                    { "MY_DEFINE", "1" },
                    { "MY_OTHER_DEFINE", "2" }
                }, 
                configFile.HlslPreprocessorDefinitions);

            var workingDirectory = Path.GetFullPath(@"Options\Assets");
            Assert.Equal(
                new string[]
                {
                    Path.Combine(workingDirectory, "ChildFolder", "Bar"),
                    Path.Combine(workingDirectory, "ChildFolder", "Foo"),
                    Path.Combine(workingDirectory, "ChildFolder"),
                    workingDirectory
                },
                configFile.HlslAdditionalIncludeDirectories);

            var root = Path.GetPathRoot(workingDirectory);
            var pathSeparator = Path.DirectorySeparatorChar.ToString();
            Assert.Equal(
                new Dictionary<string, string>
                {
                    { Path.Combine(workingDirectory, "ChildFolder", "Bar"), Path.GetFullPath("D:/read_full") },
                    { Path.Combine(workingDirectory, "ChildFolder", "Foo"), Path.Combine(workingDirectory, "ChildFolder", "GrandchildFolder", "File") },
                    { Path.GetFullPath("C:/FULL"), Path.Combine(root, "Root") },
                    { Path.Combine(workingDirectory, "ChildFolder", "GrandchildFolder", "hello"), Path.Combine(workingDirectory, "ChildFolder", "PARENT") },
                    { Path.Combine(root, "world"), Path.Combine(workingDirectory, "ChildFolder", "GrandchildFolder", "THIS") },
                    { Path.Combine(pathSeparator, "world"), Path.Combine(workingDirectory, "ChildFolder", "GrandchildFolder", "THIS") },
                    { Path.GetFullPath("D:/One/Drive"), Path.GetFullPath("C:/Another/Drive") },
                },
                configFile.HlslVirtualDirectoryMappings);
        }

        [Fact]
        public void CanLoadConfigFileWithMissingSettings()
        {
            var configFile = ConfigFileLoader.LoadAndMergeConfigFile(Path.GetFullPath(@"Options\Assets\Folder2"));

            Assert.Empty(configFile.HlslPreprocessorDefinitions);
            Assert.Empty(configFile.HlslAdditionalIncludeDirectories);
        }
    }
}