using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ShaderTools.CodeAnalysis.Options
{
    public static class ConfigFileLoader
    {
        private static readonly DataContractJsonSerializer Serializer = new DataContractJsonSerializer(typeof(ConfigFile), new DataContractJsonSerializerSettings()
        {
            UseSimpleDictionaryFormat = true
        });

        public static ConfigFile LoadAndMergeConfigFile(string directory)
        {
            if (directory == null)
                return new ConfigFile();

            if (!Path.IsPathRooted(directory))
                throw new ArgumentException();

            var configFiles = GetConfigFiles(directory);

            // We want closer config files to take precedence over further ones.

            var hlslPreprocessorDefinitions = new Dictionary<string, string>();
            foreach (var configFile in configFiles.Reverse())
                foreach (var preprocessorDefinition in configFile.HlslPreprocessorDefinitions)
                    hlslPreprocessorDefinitions[preprocessorDefinition.Key] = preprocessorDefinition.Value;

            var hlslVirtualDirectoryMappings = new Dictionary<string, string>();
            foreach (var configFile in configFiles.Reverse())
            {
                var configFileDir = Path.GetDirectoryName(configFile.FileName);
                foreach (var virtualDirectoryMapping in configFile.HlslVirtualDirectoryMappings)
                {
                    var key = CalculateFullPath(configFileDir, virtualDirectoryMapping.Key);
                    var value = CalculateFullPath(configFileDir, virtualDirectoryMapping.Value);
                    hlslVirtualDirectoryMappings[key] = value;
                }
            }

            return new ConfigFile
            {
                HlslPreprocessorDefinitions = hlslPreprocessorDefinitions,

                HlslAdditionalIncludeDirectories = configFiles
                    .SelectMany(x => x.GetAbsoluteHlslAdditionalIncludeDirectories())
                    .Distinct()
                    .ToList(),

                HlslVirtualDirectoryMappings = hlslVirtualDirectoryMappings,
            };
        }

        private static string CalculateFullPath(string basePath, string path)
        {
            var result = path
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
            if (!Path.IsPathRooted(result))
            {
                result = Path.GetFullPath(Path.Combine(basePath, result));
            }
            else
            {
                // Even if result is a rooted path, we still need to add drive letter and normalize the path
                result = Path.GetFullPath(result);
            }
            return result;
        }

        private static IEnumerable<ConfigFile> GetConfigFiles(string initialDirectory)
        {
            var directoryInfo = new DirectoryInfo(initialDirectory);

            var result = new List<ConfigFile>();
            while (true)
            {
                var configFilePath = Path.Combine(directoryInfo.FullName, "shadertoolsconfig.json");
                if (File.Exists(configFilePath))
                {
                    try
                    {
                        ConfigFile configFile;

                        // Workaround for weird DataContractJsonSerializer issue when JSON file starts with BOM.
                        var configFileText = File.ReadAllText(configFilePath);
                        using (var configFileStream = new MemoryStream(Encoding.UTF8.GetBytes(configFileText)))
                            configFile = (ConfigFile) Serializer.ReadObject(configFileStream);

                        configFile.FileName = configFilePath;
                        result.Add(configFile);

                        if (configFile.Root)
                            break;
                    }
                    catch (Exception)
                    {
                        // TODO: Log exception somewhere user can see it.
                    }
                }

                directoryInfo = directoryInfo.Parent;
                if (directoryInfo == null)
                    break;
            }
            return result;
        }
    }
}